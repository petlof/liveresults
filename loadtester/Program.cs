using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Globalization;
using System.Net.Cache;

namespace loadtester
{
    class Program
    {
        static int targetClients = 10;
        static void Main(string[] args)
        {
            Console.WriteLine("Loadtester 1.0");

            Thread mainT = new Thread(new ThreadStart(mainloop));
            mainT.Start();

            Console.WriteLine("press q to quit or enter a number to add clients");
            string msg = "";
            do
            {
                msg = Console.ReadLine();
                int numtoAdd = 0;
                if (int.TryParse(msg, out numtoAdd))
                {
                    Console.WriteLine("Adding " + numtoAdd + " clients..");
                    targetClients += numtoAdd;
                }
                
            }
            while (msg != "q");

            m_continue = false;
        }
        static bool m_continue = true;
        static void mainloop()
        {
            List<Thread> threads = new List<Thread>();
            if (File.Exists("loadtestlogg.csv"))
                File.Move("loadtestlogg.csv", "loadtestlogg.csv.bak-" + DateTime.Now.Ticks);

            File.WriteAllLines("loadtestlogg.csv", new string[] { "Time,Requests,Bytes,AvgResponseTime" });
            int lastLoggedMinute = 0;
            while (m_continue)
            {
                if (threads.Count < targetClients)
                {
                    for (int i = 0; i < Math.Max(1,targetClients - threads.Count - 10); i++)
                    {
                        Thread th = new Thread(new ThreadStart(clientThread));
                        threads.Add(th);
                        th.Start();
                    }
                }

                double rps = 0;
                double bps = 0;
                int msR = 0;

                int min = (int)(DateTime.Now.TimeOfDay.TotalMinutes);
                lock (m_stats)
                {    
                    if (!m_stats.ContainsKey(min))
                        min--;

                    int reqs = 0;
                    int bytes = 0;
                    int time = DateTime.Now.Second;
                    double totRespTime = 0;
                    if (m_stats.ContainsKey(min))
                    {
                        reqs += m_stats[min].NumRequests;
                        bytes += m_stats[min].NumBytes;
                        totRespTime += m_stats[min].ResponseTime;
                    }
                    if (m_stats.ContainsKey(min-1))
                    {
                        reqs += m_stats[min-1].NumRequests;
                        bytes += m_stats[min-1].NumBytes;
                        totRespTime += m_stats[min-1].ResponseTime;
                        time += 60;
                    }

                    rps = reqs*1.0 / time;
                    bps = bytes * 1.0 / time;
                    msR = (int)(totRespTime / reqs);

                }
                if (lastLoggedMinute+1 < min - 1)
                {
                    for (int i = lastLoggedMinute+1; i < min; i++)
                    {
                        if (m_stats.ContainsKey(i))
                        {
                            DateTime time = DateTime.Now.Date.AddMinutes(i);
                            double r = m_stats[i].NumRequests;
                            double b = m_stats[i].NumBytes;
                            double avgR = m_stats[i].ResponseTime / r;
                            File.AppendAllLines("loadtestlogg.csv", new string[] { string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm:ss},{1},{2},{3:0.00}", time, (int)r, (int)b, avgR) });
                            lastLoggedMinute = i;
                        }
                    }
                }
                
                string sbps = "";
                if (bps > 1024 * 1024 * 1024)
                    sbps = string.Format("{0:0.00}Gb/s", (bps / (1024 * 1024 * 1024)));
                else if (bps > 1024 * 1024)
                    sbps = string.Format("{0:0.00}Mb/s", (bps / (1024 * 1024)));
                else if (bps > 1024)
                    sbps = string.Format("{0:0.00}Kb/s", (bps / (1024)));
                else
                    sbps = string.Format("{0:0.00}byte/s", (bps));
                Console.Write(string.Format("[{0} threads] {1:0.00}req/s {2} {3}ms avg resp time\r", threads.Count, rps, sbps, msR));

                Thread.Sleep(1000);
            }
        }
        static Random r = new Random();
        static Dictionary<int, stats> m_stats = new Dictionary<int, stats>();
        static void clientThread()
        {
            WebClient wc = new WebClient();
            wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            string lastHash = "";
            //string klass = r.Next(0, 1) == 0 ? "W21+Elite" : "H20+E";
            string klass = r.Next(0, 1) == 0 ? "D18+E" : "H18+E";
            JavaScriptSerializer seri = new JavaScriptSerializer();
            while (m_continue)
            {
                //string url = "http://localhost/live/api.php?comp=10262&method=getclassresults&unformattedTimes=true&class=" + klass + "&last_hash=" + lastHash;
                //string url = "http://ec2-46-137-133-46.eu-west-1.compute.amazonaws.com/api.php?comp=10276&method=getclassresults&unformattedTimes=true&class=" +  klass + "&last_hash=" + lastHash;
                string url = "http://ec2-46-137-12-7.eu-west-1.compute.amazonaws.com/api.php?comp=10276&method=getclassresults&unformattedTimes=true&class=" + klass + "&last_hash=" + lastHash;
            
                DateTime start = DateTime.Now;
                //string resp = wc.DownloadString(url);
                WebRequest wq = HttpWebRequest.Create(url);
                string resp = new StreamReader(wq.GetResponse().GetResponseStream()).ReadToEnd();
                DateTime end = DateTime.Now;
                double respTime = ((TimeSpan)(end - start)).TotalMilliseconds;
                response re = (response)seri.Deserialize(resp, typeof(response));
                if (re.status == "OK")
                {
                    lastHash = re.hash;
                }
                else if (re.status == "NOT MODIFIED")
                {
                }
                else
                {
                    Console.WriteLine("Received error from server: " + resp);
                }

                lock (m_stats)
                {
                    int min = (int)(DateTime.Now.TimeOfDay.TotalMinutes);
                    if (!m_stats.ContainsKey(min))
                        m_stats.Add(min, new stats());
                    m_stats[min].NumBytes += resp.Length;
                    m_stats[min].NumRequests++;
                    m_stats[min].ResponseTime += respTime;
                }

                Thread.Sleep(15000);
            }
        }

        class response
        {
            public string status { get; set; }
            public string hash { get; set; }
        }

        class stats
        {
            public int NumRequests { get; set; }
            public int NumBytes { get; set; }
            public double ResponseTime { get; set; }
        }
    }
}
