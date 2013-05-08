using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Threading;
using System.Net;
using System.IO;

namespace LiveResults.Client
{
    public delegate void LogMessageDelegate(string msg);

   
    public class EmmaMysqlClient : IDisposable
    {
        public struct EmmaServer
        {
            public string host;
            public string user;
            public string pw;
            public string db;
        }
        public static EmmaServer[] GetServersFromConfig()
        {
            List<EmmaServer> servers = new List<EmmaServer>();
            int sNum = 1;
            while (true)
            {
                string server = ConfigurationSettings.AppSettings.Get("emmaServer" + sNum.ToString());
                if (server == null)
                    break;

                string[] parts = server.Split(';');
                EmmaServer s = new EmmaServer();
                s.host = parts[0];
                s.user = parts[1];
                s.pw = parts[2];
                s.db = parts[3];

                servers.Add(s);
                sNum++;

            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["serverpollurl"]))
            {
                WebRequest wq = HttpWebRequest.Create(ConfigurationManager.AppSettings["serverpollurl"]);
                wq.Method = "POST";
                byte[] data = Encoding.ASCII.GetBytes("key=" + ConfigurationManager.AppSettings["serverpollkey"]);
                wq.ContentLength = data.Length;
                wq.ContentType = "application/x-www-form-urlencoded";
                Stream st = wq.GetRequestStream();
                st.Write(data, 0, data.Length);
                st.Flush();
                st.Close();
                WebResponse ws = wq.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());
                string resp = sr.ReadToEnd();
                if (resp.Trim().Length > 0)
                {
                    string[] lines = resp.Trim().Split('\n');
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(';');
                        EmmaServer s = new EmmaServer();
                        s.host = parts[0];
                        s.user = parts[1];
                        s.pw = parts[2];
                        s.db = parts[3];

                        servers.Add(s);
                    }
                }
            }

            return servers.ToArray();
        }

        public event LogMessageDelegate OnLogMessage;
        private MySqlConnection m_Connection;
        private string m_ConnStr;
        private int m_CompID;
        private Hashtable m_Runners;
        private List<Runner> m_RunnersToUpdate;
        public EmmaMysqlClient(string server, int port, string user, string pass, string database, int CompetitionID)
        {
            m_Runners = new Hashtable();
            m_RunnersToUpdate = new List<Runner>();

            m_ConnStr = "Database=" + database + ";Data Source="+server+";User Id="+user+";Password="+pass;
            m_Connection = new MySqlConnection(m_ConnStr);
            m_CompID = CompetitionID;
        }

        private void resetUpdated()
        {
            foreach (Runner r in m_Runners.Values)
            {
                r.RunnerUpdated = false;
                r.ResultUpdated = false;
                r.ResetUpdatedSplits();
            }
        }

        private void FireLogMsg(string msg)
        {
            if (OnLogMessage != null)
                OnLogMessage(msg);
        }

        private bool m_Continue = false;
        private bool m_CurrentlyBuffering = false;
        private System.Threading.Thread mainTh = null;
        public void Start()
        {
            FireLogMsg("Buffering existing results..");
            int numRunners = 0;
            int numResults = 0;
            try
            {
                m_CurrentlyBuffering = true;
                m_Connection.Open();

                SetCodePage(m_Connection);

                MySqlCommand cmd = m_Connection.CreateCommand();
                cmd.CommandText = "select Runners.dbid,control,time,name,club,class,status from Runners, Results where Results.dbid = Runners.dbid and Results.tavid = " + m_CompID + " and Runners.tavid = " + m_CompID;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int dbid = (int)reader["dbid"];
                    int control = (int)reader["control"];
                    int time = (int)reader["time"];
                    if (!IsRunnerAdded(dbid))
                    {
                        Runner r = new Runner(dbid, reader["name"] as string, reader["club"] as string, reader["class"] as string);
                        AddRunner(r);
                        numRunners++;
                    }
                    if (control == 1000)
                    {
                        SetRunnerResult(dbid, time, (int)reader["status"]);
                        numResults++;
                    }
                    else if (control == 100)
                    {
                        SetRunnerStartTime(dbid, time);
                        numResults++;
                    }
                    else
                    {
                        numResults++;
                        SetRunnerSplit(dbid, control, time);
                    }
                    
                }
                reader.Close();
                cmd.Dispose();

                resetUpdated();
            }
            catch (Exception ee)
            {
                FireLogMsg(ee.Message);
                Thread.Sleep(1000);
            }
            finally
            {
                m_Connection.Close();
                m_RunnersToUpdate.Clear();
                m_CurrentlyBuffering = false;
                FireLogMsg("Done - Buffered " + m_Runners.Count + " existing runners and " + numResults +" existing results from server");
            }
            
            m_Continue = true;
            mainTh = new System.Threading.Thread(new System.Threading.ThreadStart(run));
            mainTh.Name = "Main MYSQL Thread [" + m_Connection.DataSource + "]";
            mainTh.Start();
        }

        public void UpdateRunnerInfo(int id, string name, string club, string Class)
        {
            if (m_Runners.ContainsKey(id))
            {
                Runner cur = m_Runners[id] as Runner;
                bool isUpdated = false;
                if (cur.Name != name)
                {
                    cur.Name = name;
                    isUpdated = true;
                }
                if (cur.Class != Class)
                {
                    cur.Class = Class;
                    isUpdated = true;
                }
                if (cur.Club != club)
                {
                    cur.Club = club;
                    isUpdated = true;
                }
                if (isUpdated)
                {
                    cur.RunnerUpdated = true;
                    m_RunnersToUpdate.Add(cur);

                    if (!m_CurrentlyBuffering)
                    {
                        FireLogMsg("Runnerinfo changed [" + cur.Name + "]");
                    }
                }
            }
        }

        /// <summary>
        /// Adds a Runner to this competition
        /// </summary>
        /// <param name="r"></param>
        public void AddRunner(Runner r)
        {
            if (!m_Runners.ContainsKey(r.ID))
            {
                m_Runners.Add(r.ID, r);
                m_RunnersToUpdate.Add(r);
                if (!m_CurrentlyBuffering)
                {
                    FireLogMsg("Runner added [" + r.Name + "]");
                }
            }
        }

        public int UpdatesPending
        {
            get
            {
                return m_RunnersToUpdate.Count;
            }
        }

        /// <summary>
        /// Returns true if a runner with the specified runnerid exist in the competition
        /// </summary>
        /// <param name="runnerID"></param>
        /// <returns></returns>
        public bool IsRunnerAdded(int runnerID)
        {
            return m_Runners.ContainsKey(runnerID);
        }

        /// <summary>
        /// Sets the result for the runner with runnerID
        /// </summary>
        /// <param name="runnerID"></param>
        /// <param name="control"></param>
        /// <param name="time"></param>
        /// <param name="status"></param>
        public void SetRunnerResult(int runnerID, int time, int status)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerResult]");

            Runner r = (Runner)m_Runners[runnerID];

            if (r.HasResultChanged(time, status))
            {
                r.SetResult(time, status);
                m_RunnersToUpdate.Add(r);
                if (!m_CurrentlyBuffering)
                {
                    FireLogMsg("Runner result changed: [" + r.Name + ", " + r.Time + "]");
                }
            }
        }

        public void SetRunnerSplit(int runnerID, int controlcode, int time)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerResult]");
            Runner r = (Runner)m_Runners[runnerID];

            if (r.HasSplitChanged(controlcode, time))
            {
                r.SetSplitTime(controlcode, time);
                m_RunnersToUpdate.Add(r);
                if (!m_CurrentlyBuffering)
                {
                    FireLogMsg("Runner Split Changes: [" + r.Name + ", {cn: " + controlcode + ", t: " + time + "}]");
                }
            }

        }

        public void SetRunnerStartTime(int runnerID, int starttime)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerStartTime]");
            Runner r = (Runner)m_Runners[runnerID];

            if (r.HasStartTimeChanged(starttime))
            {
                r.SetStartTime(starttime);
                m_RunnersToUpdate.Add(r);
                if (!m_CurrentlyBuffering)
                {
                    FireLogMsg("Runner starttime Changed: [" + r.Name + ", t: " + starttime + "}]");
                }
            }

        }

        public void Stop()
        {
            m_Continue = false;
        }

        private void run()
        {
            while (m_Continue)
            {
                try
                {
                    m_Connection = new MySqlConnection(m_ConnStr);
                    m_Connection.Open();
                    SetCodePage(m_Connection);
                    while (m_Continue)
                    {
                        if (m_RunnersToUpdate.Count > 0)
                        {
                            using (MySqlCommand cmd = m_Connection.CreateCommand())
                            {
                                Runner r = m_RunnersToUpdate[0];
                                if (r.RunnerUpdated)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("?compid", m_CompID);
                                    cmd.Parameters.AddWithValue("?name", Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(r.Name))));
                                    cmd.Parameters.AddWithValue("?club", Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(r.Club))));
                                    cmd.Parameters.AddWithValue("?class", Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(r.Class))));
                                    cmd.Parameters.AddWithValue("?id", r.ID);
                                    //cmd.CommandText = "REPLACE INTO Runners VALUES (" + m_CompID + ",'" + r.Name.Replace('\'','`') + "','" + r.Club + "','" + r.Class + "',0," + r.ID + ")";
                                    cmd.CommandText = "REPLACE INTO Runners VALUES (?compid,?name,?club,?class,0,?id)";

                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ee)
                                    {
                                        //Move failing runner last
                                        m_RunnersToUpdate.Add(r);
                                        m_RunnersToUpdate.RemoveAt(0);
                                        throw new ApplicationException("Could not add runner " + r.Name + ", " + r.Club + ", " + r.Class + " to server due to: " + ee.Message, ee);
                                    }
                                    cmd.Parameters.Clear();
                                    FireLogMsg("Runner " + r.Name + " updated in DB");
                                    r.RunnerUpdated = false;
                                }
                                if (r.ResultUpdated)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("?compid", m_CompID);
                                    cmd.Parameters.AddWithValue("?id", r.ID);
                                    cmd.Parameters.AddWithValue("?time", r.Time);
                                    cmd.Parameters.AddWithValue("?status", r.Status);
                                    cmd.CommandText = "REPLACE INTO Results VALUES(?compid,?id,1000,?time,?status,Now())";
                                    cmd.ExecuteNonQuery();
                                    cmd.Parameters.Clear();

                                    FireLogMsg("Runner " + r.Name + "s result updated in DB");
                                    r.ResultUpdated = false;
                                }
                                if (r.StartTimeUpdated)
                                {
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("?compid", m_CompID);
                                    cmd.Parameters.AddWithValue("?id", r.ID);
                                    cmd.Parameters.AddWithValue("?starttime", r.StartTime);
                                    cmd.Parameters.AddWithValue("?status", r.Status);
                                    //cmd.CommandText = "REPLACE INTO Results VALUES(" + m_CompID + "," + r.ID + ",0," + r.StartTime + "," + r.Status + ",Now())";
                                    cmd.CommandText = "REPLACE INTO Results VALUES(?compid,?id,100,?starttime,?status,Now())";
                                    cmd.ExecuteNonQuery();
                                    cmd.Parameters.Clear();
                                    FireLogMsg("Runner " + r.Name + "s starttime updated in DB");
                                    r.StartTimeUpdated = false;
                                }
                                if (r.HasUpdatedSplitTimes())
                                {
                                    List<SplitTime> splitTimes = r.GetUpdatedSplitTimes();

                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("?compid", m_CompID);
                                    cmd.Parameters.AddWithValue("?id", r.ID);
                                    cmd.Parameters.AddWithValue("?control", -1);
                                    cmd.Parameters.AddWithValue("?time", -1);
                                    foreach (SplitTime t in splitTimes)
                                    {
                                        cmd.Parameters["?control"].Value = t.Control;
                                        cmd.Parameters["?time"].Value = t.Time;
                                        cmd.CommandText = "REPLACE INTO Results VALUES(" + m_CompID + "," + r.ID + "," + t.Control + "," + t.Time + ",0,Now())";
                                        cmd.ExecuteNonQuery();
                                        t.Updated = false;
                                        FireLogMsg("Runner " + r.Name + " splittime{" + t.Control + "} updated in DB");
                                    }
                                    cmd.Parameters.Clear();
                                }

                                m_RunnersToUpdate.RemoveAt(0);
                            }
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }

                }
                catch (Exception ee)
                {
                    FireLogMsg("Error: " + ee.Message + " [" + m_Connection.DataSource + "]");
                    System.Diagnostics.Debug.Write(ee.Message);
                    System.Threading.Thread.Sleep(1000);
                }
                finally
                {
                    m_Connection.Close();
                    m_Connection.Dispose();
                    m_Connection = null;
                }
            }
        }

        private void SetCodePage(MySqlConnection conn)
        {
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "set names 'utf8'";
                cmd.ExecuteNonQuery();
            }
        }

        public override string ToString()
        {
            return (m_Connection != null ? m_Connection.DataSource : "Detached") + " (" + UpdatesPending + ")";
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (m_Connection != null)
            {
                m_Connection.Close();
                m_Connection.Dispose();
            }
        }

        #endregion
    }
}
