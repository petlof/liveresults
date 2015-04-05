using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpSirapTest
{
    class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Identifying liveresults client sirap interface");
            var udpClient = new UdpClient();
            var data = Encoding.ASCII.GetBytes("LiveResClientIdentify");
            udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 44563));

            var remote = new IPEndPoint(IPAddress.Any, 0);

            var resc = udpClient.Receive(ref remote);
            var decoded = Encoding.ASCII.GetString(resc);
            Console.WriteLine("Got server: " + decoded);
            Console.Read();
        }
    }
}
