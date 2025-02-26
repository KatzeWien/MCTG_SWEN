// See https://aka.ms/new-console-template for more information
using MonsterTrading;
using System.Net;

IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
int port = 10001;

Server server = new Server(iPAddress, port);
server.Start();
