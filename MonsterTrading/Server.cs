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
        private DBAccess dbAccess;
        public Server(IPAddress address, int port)
        {
            this.httpServer = new TcpListener(address, port);
            dbAccess = new DBAccess();
        }

        public void Start()
        {
            httpServer.Start();
            dbAccess.Connect();
            while (true)
            {
                var clientSocket = httpServer.AcceptTcpClient();
                Console.WriteLine("Hello");
            }
        }
    }
}