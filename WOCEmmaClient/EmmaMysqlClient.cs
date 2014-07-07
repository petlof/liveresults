using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace LiveResults.Client
{
    public delegate void LogMessageDelegate(string msg);

   
    public class EmmaMysqlClient : IDisposable
    {
        public struct EmmaServer
        {
            public string Host;
            public string User;
            public string Pw;
            public string DB;
        }
        public static EmmaServer[] GetServersFromConfig()
        {
            var servers = new List<EmmaServer>();
            int sNum = 1;
            while (true)
            {
                string server = ConfigurationManager.AppSettings["emmaServer" + sNum];
                if (server == null)
                    break;

                string[] parts = server.Split(';');
                var s = new EmmaServer{
                    Host = parts[0],
                    User = parts[1],
                    Pw = parts[2],
                    DB = parts[3]
                };

                servers.Add(s);
                sNum++;

            }
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["serverpollurl"]))
            {
                WebRequest wq = WebRequest.Create(ConfigurationManager.AppSettings["serverpollurl"]);
                wq.Method = "POST";
                byte[] data = Encoding.ASCII.GetBytes("key=" + ConfigurationManager.AppSettings["serverpollkey"]);
                wq.ContentLength = data.Length;
                wq.ContentType = "application/x-www-form-urlencoded";
                Stream st = wq.GetRequestStream();
                st.Write(data, 0, data.Length);
                st.Flush();
                st.Close();
                WebResponse ws = wq.GetResponse();
                Stream responseStream = ws.GetResponseStream();
                if (responseStream != null)
                {
                    var sr = new StreamReader(responseStream);
                    string resp = sr.ReadToEnd();
                    if (resp.Trim().Length > 0)
                    {
                        string[] lines = resp.Trim().Split('\n');
                        foreach (string line in lines)
                        {
                            string[] parts = line.Split(';');
                            var s = new EmmaServer{
                                Host = parts[0],
                                User = parts[1],
                                Pw = parts[2],
                                DB = parts[3]
                            };

                            servers.Add(s);
                        }
                    }
                }
            }

            return servers.ToArray();
        }

        public event LogMessageDelegate OnLogMessage;
        private MySqlConnection m_connection;
        private readonly string m_connStr;
        private readonly int m_compID;
        private readonly Dictionary<int,Runner> m_runners;
        private readonly List<DbItem> m_itemsToUpdate;
        public EmmaMysqlClient(string server, int port, string user, string pass, string database, int competitionID)
        {
            m_runners = new Dictionary<int, Runner>();
            m_itemsToUpdate = new List<DbItem>();

            m_connStr = "Database=" + database + ";Data Source="+server+";User Id="+user+";Password="+pass;
            m_connection = new MySqlConnection(m_connStr);
            m_compID = competitionID;
        }

        private void ResetUpdated()
        {
            foreach (Runner r in m_runners.Values)
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

        public Runner GetRunner(int dbId)
        {
            if (!IsRunnerAdded(dbId))
                return null;

            return m_runners[dbId];
        }

        private bool m_continue;
        private bool m_currentlyBuffering;
        private Thread m_mainTh;
        public void Start()
        {
            FireLogMsg("Buffering existing results..");
            int numResults = 0;
            try
            {
                m_currentlyBuffering = true;
                m_connection.Open();

                SetCodePage(m_connection);

                MySqlCommand cmd = m_connection.CreateCommand();
                cmd.CommandText = "select runners.dbid,control,time,name,club,class,status from runners, results where results.dbid = runners.dbid and results.tavid = " + m_compID + " and runners.tavid = " + m_compID;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var dbid = (int)reader["dbid"];
                    var control = (int)reader["control"];
                    var time = (int)reader["time"];
                    if (!IsRunnerAdded(dbid))
                    {
                        var r = new Runner(dbid, reader["name"] as string, reader["club"] as string, reader["class"] as string);
                        AddRunner(r);
                    }
                    switch (control)
                    {
                        case 1000:
                            SetRunnerResult(dbid, time, (int)reader["status"]);
                            numResults++;
                            break;
                        case 100:
                            SetRunnerStartTime(dbid, time);
                            numResults++;
                            break;
                        default:
                            numResults++;
                            SetRunnerSplit(dbid, control, time);
                            break;
                    }
                    
                }
                reader.Close();
                cmd.Dispose();

                ResetUpdated();
            }
            catch (Exception ee)
            {
                FireLogMsg(ee.Message);
                Thread.Sleep(1000);
            }
            finally
            {
                m_connection.Close();
                m_itemsToUpdate.Clear();
                m_currentlyBuffering = false;
                FireLogMsg("Done - Buffered " + m_runners.Count + " existing runners and " + numResults +" existing results from server");
            }
            
            m_continue = true;
            m_mainTh = new Thread(Run);
            m_mainTh.Name = "Main MYSQL Thread [" + m_connection.DataSource + "]";
            m_mainTh.Start();
        }

        public void UpdateRunnerInfo(int id, string name, string club, string Class)
        {
            if (m_runners.ContainsKey(id))
            {
                var cur = m_runners[id];
                if (cur == null)
                    return;
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
                    m_itemsToUpdate.Add(cur);

                    if (!m_currentlyBuffering)
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
            if (!m_runners.ContainsKey(r.ID))
            {
                m_runners.Add(r.ID, r);
                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireLogMsg("Runner added [" + r.Name + "]");
                }
            }
        }

        public void SetRadioControl(string className, int code, string controlName, int order)
        {
            m_itemsToUpdate.Add(new RadioControl
            {
                ClassName = className,
                Code = code,
                ControlName = controlName,
                Order = order
            });
        }

        public int UpdatesPending
        {
            get
            {
                return m_itemsToUpdate.Count;
            }
        }

        /// <summary>
        /// Returns true if a runner with the specified runnerid exist in the competition
        /// </summary>
        /// <param name="runnerID"></param>
        /// <returns></returns>
        public bool IsRunnerAdded(int runnerID)
        {
            return m_runners.ContainsKey(runnerID);
        }

        /// <summary>
        /// Sets the result for the runner with runnerID
        /// </summary>
        /// <param name="runnerID"></param>
        /// <param name="time"></param>
        /// <param name="status"></param>
        public void SetRunnerResult(int runnerID, int time, int status)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerResult]");

            var r = m_runners[runnerID];

            if (r.HasResultChanged(time, status))
            {
                r.SetResult(time, status);
                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireLogMsg("Runner result changed: [" + r.Name + ", " + r.Time + "]");
                }
            }
        }

        public void SetRunnerSplit(int runnerID, int controlcode, int time)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerResult]");
            var r = m_runners[runnerID];

            if (r.HasSplitChanged(controlcode, time))
            {
                r.SetSplitTime(controlcode, time);
                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireLogMsg("Runner Split Changes: [" + r.Name + ", {cn: " + controlcode + ", t: " + time + "}]");
                }
            }

        }

        public void SetRunnerStartTime(int runnerID, int starttime)
        {
            if (!IsRunnerAdded(runnerID))
                throw new ApplicationException("Runner is not added! {" + runnerID + "} [SetRunnerStartTime]");
            var r = m_runners[runnerID];

            if (r.HasStartTimeChanged(starttime))
            {
                r.SetStartTime(starttime);
                m_itemsToUpdate.Add(r);
                if (!m_currentlyBuffering)
                {
                    FireLogMsg("Runner starttime Changed: [" + r.Name + ", t: " + starttime + "}]");
                }
            }

        }

        public void MergeRunners(Runner[] runners)
        {
            if (runners == null)
                return;

            foreach (var r in runners)
            {
                if (!IsRunnerAdded(r.ID))
                {
                    AddRunner(new Runner(r.ID, r.Name, r.Club, r.Class));
                }
                else
                {
                    UpdateRunnerInfo(r.ID, r.Name, r.Club, r.Class);
                }


                if (r.StartTime >= 0)
                    SetRunnerStartTime(r.ID, r.StartTime);

                SetRunnerResult(r.ID, r.Time, r.Status);

                var spl = r.SplitTimes;
                if (spl != null)
                {
                    foreach (var s in spl)
                    {
                        SetRunnerSplit(r.ID, s.Control, s.Time);
                    }
                }
            }
        }


        public void Stop()
        {
            m_continue = false;
        }

        private void Run()
        {
            while (m_continue)
            {
                try
                {
                    m_connection = new MySqlConnection(m_connStr);
                    m_connection.Open();
                    SetCodePage(m_connection);
                    while (m_continue)
                    {
                        if (m_itemsToUpdate.Count > 0)
                        {
                            using (MySqlCommand cmd = m_connection.CreateCommand())
                            {
                                var item = m_itemsToUpdate[0];
                                if (item is RadioControl)
                                {
                                    var r = item as RadioControl;
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("?compid", m_compID);
                                    cmd.Parameters.AddWithValue("?name", r.ClassName);
                                    cmd.Parameters.AddWithValue("?corder", r.Order);
                                    cmd.Parameters.AddWithValue("?code", r.Code);
                                    cmd.Parameters.AddWithValue("?cname", r.ControlName);
                                    cmd.CommandText = "REPLACE INTO splitcontrols VALUES (?compid,?name,?corder,?code,?cname)";

                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception ee)
                                    {
                                        //Move failing runner last
                                        m_itemsToUpdate.Add(r);
                                        m_itemsToUpdate.RemoveAt(0);
                                        throw new ApplicationException("Could not add radiocontrol " + r.ControlName + ", " + r.ClassName + ", " + r.Code + " to server due to: " + ee.Message, ee);
                                    }
                                    cmd.Parameters.Clear();
                                }
                                else if (item is Runner)
                                {
                                    var r = item as Runner;
                                    if (r.RunnerUpdated)
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("?compid", m_compID);
                                        cmd.Parameters.AddWithValue("?name", r.Name);
                                        cmd.Parameters.AddWithValue("?club", r.Club);
                                        cmd.Parameters.AddWithValue("?class", r.Class);

                                        cmd.Parameters.AddWithValue("?id", r.ID);
                                        cmd.CommandText = "REPLACE INTO runners VALUES (?compid,?name,?club,?class,0,?id)";

                                        try
                                        {
                                            cmd.ExecuteNonQuery();
                                        }
                                        catch (Exception ee)
                                        {
                                            //Move failing runner last
                                            m_itemsToUpdate.Add(r);
                                            m_itemsToUpdate.RemoveAt(0);
                                            throw new ApplicationException(
                                                "Could not add runner " + r.Name + ", " + r.Club + ", " + r.Class + " to server due to: " + ee.Message, ee);
                                        }
                                        cmd.Parameters.Clear();
                                        FireLogMsg("Runner " + r.Name + " updated in DB");
                                        r.RunnerUpdated = false;
                                    }
                                    if (r.ResultUpdated)
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("?compid", m_compID);
                                        cmd.Parameters.AddWithValue("?id", r.ID);
                                        cmd.Parameters.AddWithValue("?time", r.Time);
                                        cmd.Parameters.AddWithValue("?status", r.Status);
                                        cmd.CommandText = "REPLACE INTO results VALUES(?compid,?id,1000,?time,?status,Now())";
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();

                                        FireLogMsg("Runner " + r.Name + "s result updated in DB");
                                        r.ResultUpdated = false;
                                    }
                                    if (r.StartTimeUpdated)
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("?compid", m_compID);
                                        cmd.Parameters.AddWithValue("?id", r.ID);
                                        cmd.Parameters.AddWithValue("?starttime", r.StartTime);
                                        cmd.Parameters.AddWithValue("?status", r.Status);
                                        //cmd.CommandText = "REPLACE INTO Results VALUES(" + m_CompID + "," + r.ID + ",0," + r.StartTime + "," + r.Status + ",Now())";
                                        cmd.CommandText = "REPLACE INTO results VALUES(?compid,?id,100,?starttime,?status,Now())";
                                        cmd.ExecuteNonQuery();
                                        cmd.Parameters.Clear();
                                        FireLogMsg("Runner " + r.Name + "s starttime updated in DB");
                                        r.StartTimeUpdated = false;
                                    }
                                    if (r.HasUpdatedSplitTimes())
                                    {
                                        List<SplitTime> splitTimes = r.GetUpdatedSplitTimes();

                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("?compid", m_compID);
                                        cmd.Parameters.AddWithValue("?id", r.ID);
                                        cmd.Parameters.AddWithValue("?control", -1);
                                        cmd.Parameters.AddWithValue("?time", -1);
                                        foreach (SplitTime t in splitTimes)
                                        {
                                            cmd.Parameters["?control"].Value = t.Control;
                                            cmd.Parameters["?time"].Value = t.Time;
                                            cmd.CommandText = "REPLACE INTO results VALUES(" + m_compID + "," + r.ID + "," + t.Control + "," + t.Time +
                                                              ",0,Now())";
                                            cmd.ExecuteNonQuery();
                                            t.Updated = false;
                                            FireLogMsg("Runner " + r.Name + " splittime{" + t.Control + "} updated in DB");
                                        }
                                        cmd.Parameters.Clear();
                                    }
                                }

                                m_itemsToUpdate.RemoveAt(0);
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                }
                catch (Exception ee)
                {
                    FireLogMsg("Error: " + ee.Message + (m_connection != null ? " [" + m_connection.DataSource + "]" : ""));
                    System.Diagnostics.Debug.Write(ee.Message);
                    Thread.Sleep(1000);
                }
                finally
                {
                    if (m_connection != null)
                    {
                        m_connection.Close();
                        m_connection.Dispose();
                        m_connection = null;
                    }
                }
            }
        }

        private void SetCodePage(MySqlConnection conn)
        {
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "set names 'utf8'";
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["server_charset"]))
                {
                    cmd.CommandText = "set names '" + ConfigurationManager.AppSettings["server_charset"] + "'";
                }
                cmd.ExecuteNonQuery();
            }
        }

        public override string ToString()
        {
            return (m_connection != null ? m_connection.DataSource : "Detached") + " (" + UpdatesPending + ")";
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (m_connection != null)
            {
                m_connection.Close();
                m_connection.Dispose();
            }
        }

        #endregion
    }
}
