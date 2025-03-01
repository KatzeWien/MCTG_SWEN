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
        private UserDB userDB;
        private string? path;
        private string? method;
        private string? body;

        public Server(IPAddress address, int port)
        {
            this.httpServer = new TcpListener(address, port);
            dbAccess = new DBAccess();
            userDB = new UserDB();
        }

        public void Start()
        {
            httpServer.Start();
            while (true)
            {
                var clientSocket = httpServer.AcceptTcpClient();
                Console.WriteLine("Hello");
                GetEndpoint(clientSocket);
            }
        }

        public void GetEndpoint(TcpClient clientSocket)
        {
            using var reader = new StreamReader(clientSocket.GetStream());
            analyseRequest(reader);
            if(this.path == "/users")
            {
                if(this.method == "GET")
                {
                    Console.WriteLine("get");
                    this.userDB.ShowAllUser();
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                    if (this.body != null)
                    {
                        this.userDB.CreateUser(this.body);
                    }
                }
            }
            else if(this.path == "/sessions")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if(this.path == "/packages")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if(this.path == "/transactions/packages")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if (this.path == "/cards")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if (this.path == "/deck")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if (this.path == "/scoreboard")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if (this.path == "/battles")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if (this.path == "/stats")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
            else if (this.path == "/tradings")
            {
                if (this.method == "GET")
                {
                    Console.WriteLine("get");
                }
                else if (this.method == "POST" || this.method == "PUT" || this.method == "DELETE")
                {
                    Console.WriteLine("update");
                }
            }
        }

        public void analyseRequest(StreamReader reader)
        {
            // ----- 1. Read the HTTP-Request -----
            string? line;

            // 1.1 first line in HTTP contains the method, path and HTTP version
            line = reader.ReadLine();
            if (line != null)
            {
                var partsOfFirstLine = line.Split(" ");
                this.method = partsOfFirstLine[0];
                this.path = partsOfFirstLine[1];
            }
            // 1.2 read the HTTP-headers (in HTTP after the first line, until the empy line)
            int content_length = 0; // we need the content_length later, to be able to read the HTTP-content
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "")
                {
                    break;  // empty line indicates the end of the HTTP-headers
                }

                // Parse the header
                var parts = line.Split(':');
                if (parts.Length == 2 && parts[0].Trim() == "Content-Length")
                {
                    content_length = int.Parse(parts[1].Trim());
                }
            }
            // 1.3 read the body if existing
            if (content_length > 0)
            {
                var data = new StringBuilder(200);
                char[] chars = new char[1024];
                int bytesReadTotal = 0;
                while (bytesReadTotal < content_length)
                {
                    var bytesRead = reader.Read(chars, 0, chars.Length);
                    bytesReadTotal += bytesRead;
                    if (bytesRead == 0)
                        break;
                    data.Append(chars, 0, bytesRead);
                }
                this.body = (data.ToString());
            }
        }
    }
}