using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LiveResults.Client
{
    public class SirapInterface : IDisposable
    {

        private TcpListener m_listener;
        private UdpClient m_udpClient;
        private bool m_continue;
        public void Start()
        {
            m_continue = true;
            m_udpClient = new UdpClient(44563);
            m_listener = new TcpListener(IPAddress.Any, 0);
            m_listener.Start();

            //Thread for responding on UDP BroadCasts trying to find this liveresults client instance..
            ThreadPool.QueueUserWorkItem(delegate
            {
                var strHostName = Dns.GetHostName();
                Debug.WriteLine("Local Machine's Host Name: " + strHostName);
                // Then using host name, get the IP address list..
                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
                IPAddress[] addresses = ipEntry.AddressList;
                IPAddress address = addresses.FirstOrDefault(t => t.AddressFamily == AddressFamily.InterNetwork);

                var remoteClient = new IPEndPoint(IPAddress.Any, 0);

                if (address != null)
                {
                    var respData = System.Text.Encoding.ASCII.GetBytes(address + ";" + ((IPEndPoint) m_listener.LocalEndpoint).Port);


                    while (m_continue)
                    {
                        var reqData = m_udpClient.Receive(ref remoteClient);
                        var decodedData = System.Text.Encoding.ASCII.GetString(reqData);
                        if (decodedData == "LiveResClientIdentify")
                        {
                            Debug.WriteLine("Got IdentifyRequest from: " + remoteClient.Address);
                            m_udpClient.Send(respData, respData.Length, remoteClient);
                        }
                    }

                }
            });


        }

        public void Stop()
        {
            m_continue = false;
            if (m_listener != null)
            {
                m_listener.Stop();
                m_listener = null;
            }
        }


        public void Dispose()
        {
            Stop();
        }
    }
}
