using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LiveResults.Client.Interfaces
{
    public class TcpIpInterface
    {
        private readonly TcpListener m_listener;
        private bool m_continue = false;
        private Thread m_listernerThread;

        public TcpIpInterface()
        {
            m_listener = new TcpListener(IPAddress.Any, 42366);
        }

        public void Start()
        {
            m_listener.Start();
            m_continue = true;


        }

        public void Stop()
        {
            m_continue = false;
            m_listener.Stop();
        }

    }
}
