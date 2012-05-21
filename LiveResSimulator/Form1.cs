using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Threading;
using System.Net;
using System.Web;

namespace LiveResSimulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            if (File.Exists("setts.dat"))
            {
                XmlSerializer s = new XmlSerializer(typeof(Settings));
                FileStream fs = File.OpenRead("setts.dat");
                var setts = (Settings)s.Deserialize(fs);
                fs.Close();
                textBox2.Text = setts.targetDatasource;
                textBox3.Text = setts.startlistURL;
            }
        }

        private void button1_Click(object sender, EventArgs args)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                BinaryFormatter bf = new BinaryFormatter();
                List<Runner> runners = new List<Runner>();
                Event e = new Event();
                List<Result> results = new List<Result>();
                List<RadioControl> radious = new List<RadioControl>();

                
                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection("");
                conn.Open();
                MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select * from runners where tavid=" + textBox1.Text;
                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    runners.Add(new Runner()
                    {
                        Name = reader["name"] as string,
                        Class = reader["class"] as string,
                        club = reader["club"] as string,
                        dbId = Convert.ToInt32(reader["dbid"])

                    });
                }
                e.Runners = runners.ToArray();
                reader.Close();

                cmd.CommandText = "select * from results where tavid=" + textBox1.Text;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new Result()
                    {
                        control = Convert.ToInt32(reader["control"]),
                        status = Convert.ToInt32(reader["status"]),
                        time = Convert.ToInt32(reader["time"]),
                        changed = Convert.ToDateTime(reader["changed"]),
                        dbId = Convert.ToInt32(reader["dbid"])

                    });
                }
                e.Results = results.ToArray();
                reader.Close();

                cmd.CommandText = "select * from splitcontrols where tavid=" + textBox1.Text;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    radious.Add(new RadioControl()
                    {
                        classname = reader["classname"] as string,
                         code = Convert.ToInt32(reader["code"]),
                          corder = Convert.ToInt32(reader["corder"]),
                           name = reader["name"] as string
                    });
                }
                e.RadioControls = radious.ToArray();
                reader.Close();

                cmd.CommandText = "select * from login where tavid=" + textBox1.Text;
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    e.CompDate = Convert.ToDateTime(reader["compDate"]);
                    e.CompId = Convert.ToInt32(textBox1.Text);
                    e.CompName = reader["compname"] as string;
                    e.organizer = reader["organizer"] as string;
                    e.Public = Convert.ToBoolean(reader["public"]);

                }
                reader.Close();

                FileStream fs = File.Create(sfd.FileName);
                bf.Serialize(fs, e);
                fs.Close();
                conn.Close();
            }
        }

        Event replayEvent = null;
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = File.OpenRead(ofd.FileName);
                replayEvent = (Event)bf.Deserialize(fs);
                fs.Close();
                lblInfo.Text = "ID: " + replayEvent.CompId + "\r\n" + "Name: " + replayEvent.CompName + "\r\n" + "Start: " + replayEvent.Results.Min(x => x.changed).ToString("yyyy-MM-hh HH:mm:ss") + "\r\n" + "End: " + replayEvent.Results.Max(x => x.changed).ToString("yyyy-MM-hh HH:mm:ss");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                XmlSerializer s = new XmlSerializer(typeof(Settings));
                FileStream fs = File.Create("setts.dat");
                s.Serialize(fs, new Settings() { targetDatasource = textBox2.Text, startlistURL = textBox3.Text });
                fs.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        [Serializable]
        public class Settings
        {
            public string targetDatasource { get; set; }
            public string startlistURL { get; set; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(this, e.Error.Message);
            }
            else
            {
                lblTime.Text = "Stopped";
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            trackBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(textBox2.Text);
            try
            {
                conn.Open();
                MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                double numSeconds = ((TimeSpan)(replayEvent.Results.Max(x => x.changed) - replayEvent.Results.Min(x => x.changed))).TotalSeconds;
                cmd.CommandText = "delete from login where tavid=" + replayEvent.CompId;
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into login(tavid,user,pass,compName,organizer,compDate,public) values(?id,?user,?pass,?compName,?organizer,?compdate,?public)";
                cmd.Parameters.AddWithValue("?id", replayEvent.CompId);
                cmd.Parameters.AddWithValue("?user", "");
                cmd.Parameters.AddWithValue("?pass", "");
                cmd.Parameters.AddWithValue("?compName", replayEvent.CompName);
                cmd.Parameters.AddWithValue("?organizer", replayEvent.organizer);
                cmd.Parameters.AddWithValue("?compDate", replayEvent.CompDate);
                cmd.Parameters.AddWithValue("?public", replayEvent.Public);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();


                cmd.CommandText = "delete from splitcontrols where tavid=" + replayEvent.CompId;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into splitcontrols(tavid,classname,name,code,corder) values(?id,?class,?name,?code,?corder)";
                cmd.Parameters.AddWithValue("?id", replayEvent.CompId);
                cmd.Parameters.AddWithValue("?class", "");
                cmd.Parameters.AddWithValue("?name", "");
                cmd.Parameters.AddWithValue("?code", 0);
                cmd.Parameters.AddWithValue("?corder", 0);
                foreach (var r in replayEvent.RadioControls)
                {
                    cmd.Parameters["?class"].Value = r.classname;
                    cmd.Parameters["?name"].Value = r.name;
                    cmd.Parameters["?code"].Value = r.code;
                    cmd.Parameters["?corder"].Value = r.corder;
                    cmd.ExecuteNonQuery();
                }
                cmd.Parameters.Clear();

                cmd.CommandText = "delete from runners where tavid=" + replayEvent.CompId;
                cmd.ExecuteNonQuery();


                cmd.CommandText = "delete from results where tavid=" + replayEvent.CompId;
                cmd.ExecuteNonQuery();

                WebClient wc = new WebClient();
                


                cmd.Parameters.AddWithValue("?compid", replayEvent.CompId);
                cmd.Parameters.AddWithValue("?name", "");
                cmd.Parameters.AddWithValue("?club", "");
                cmd.Parameters.AddWithValue("?class", "");
                cmd.Parameters.AddWithValue("?id", 0);
                cmd.CommandText = "REPLACE INTO runners VALUES (?compid,?name,?club,?class,0,?id)";
                foreach (var r in replayEvent.Runners)
                {
                    cmd.Parameters["?name"].Value = r.Name;
                    cmd.Parameters["?club"].Value = r.club;
                    cmd.Parameters["?class"].Value = r.Class;
                    cmd.Parameters["?id"].Value = r.dbId;
                    cmd.ExecuteNonQuery();
                }
                cmd.Parameters.Clear();

                
                cmd.Parameters.AddWithValue("?compid", replayEvent.CompId);
                cmd.Parameters.AddWithValue("?id", 0);
                cmd.Parameters.AddWithValue("?control", -1);
                cmd.Parameters.AddWithValue("?time", -1);
                cmd.Parameters.AddWithValue("?status", -1);
                cmd.CommandText = "REPLACE INTO Results VALUES(?compid,?id,?control,?time,?status,Now())";


                if (!string.IsNullOrEmpty(textBox3.Text))
                {
                    string startlist;
                    startlist = HttpUtility.HtmlDecode(wc.DownloadString(textBox3.Text));
                    foreach (var r in replayEvent.Runners)
                    {
                        int rIdx = startlist.IndexOf(r.Name);
                        if (rIdx > 0)
                        {
                            int sIdx = startlist.IndexOf(" class=\"t\">", rIdx);
                            sIdx += 11;
                            int eIdx = startlist.IndexOf("</td>", sIdx);
                            string start = startlist.Substring(sIdx, eIdx - sIdx);

                            string[] p = start.Split(':');
                            int starttime = 0;
                            if (p.Length == 3)
                            {
                                starttime = Convert.ToInt32(p[0]) * 360000
                                    + Convert.ToInt32(p[1]) * 6000 +
                                        +Convert.ToInt32(p[2]) * 100;
                            }
                            else
                            {
                                starttime = Convert.ToInt32(p[0]) * 360000
                                    + Convert.ToInt32(p[1]) * 6000;
                            }

                            cmd.Parameters["?id"].Value = r.dbId;
                            cmd.Parameters["?control"].Value = 100;
                            cmd.Parameters["?time"].Value = starttime;
                            cmd.Parameters["?status"].Value = 0;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                do
                {
                    DateTime clock = replayEvent.Results.Min(x => x.changed);
                    DateTime endClock = replayEvent.Results.Max(x => x.changed);

                    IDbCommand cmd2 = conn.CreateCommand();
                    cmd2.CommandText = "delete from results where tavid = " + replayEvent.CompId + " and control <> 100";
                    cmd2.ExecuteNonQuery();

                    while (clock <= endClock)
                    {
                        if (backgroundWorker1.CancellationPending)
                            break;

                        var next = replayEvent.Results.Where(x => x.changed >= clock).OrderBy(x => x.changed).First().changed;
                        IEnumerable<Result> changes = null;
                        if (((TimeSpan)(next - clock)).TotalMinutes > 10)
                        {
                            changes = replayEvent.Results.Where(x => x.changed >= clock && x.changed <= next);
                            clock = next;
                        }
                        else
                        {
                            changes = replayEvent.Results.Where(x => x.changed == clock);
                        }

                        lblTime.Invoke(new MethodInvoker(delegate
                            {
                                lblTime.Text = clock.ToString("yyyy-MM-dd HH:mm:ss");
                            }));



                       
                        foreach (var c in changes)
                        {
                            cmd.Parameters["?id"].Value = c.dbId;
                            cmd.Parameters["?control"].Value = c.control;
                            cmd.Parameters["?time"].Value = c.time;
                            cmd.Parameters["?status"].Value = c.status;
                            cmd.ExecuteNonQuery();
                        }

                        if (jumpToNext)
                        {
                            clock = replayEvent.Results.Where(x => x.changed > clock).OrderBy(x => x.changed).First().changed;
                            jumpToNext = false;
                        }
                        else
                        {
                            clock = clock.AddSeconds(1);
                        }
                        Thread.Sleep(1000);
                    }
                } while (!backgroundWorker1.CancellationPending);
            }
            finally
            {
                conn.Close();
            }

        }
        bool jumpToNext = false;
        private void button5_Click(object sender, EventArgs e)
        {
            jumpToNext = true;
        }
    }
}
