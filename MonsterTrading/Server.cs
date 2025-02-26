using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MonsterTrading
{
    public class Server
    {
        private TcpListener httpServer;
        public Server(IPAddress address, int port)
        {
            this.httpServer = new TcpListener(address, port);
        }

        public void Start()
        {
            httpServer.Start();
            while (true)
            {
                var clientSocket = httpServer.AcceptTcpClient();
                Console.WriteLine("Hello");
            }
        }
    }
}