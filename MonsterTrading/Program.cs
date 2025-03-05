using MonsterTrading.Server;
using System.Net;
using System.Net.Sockets;

internal class Program
{
    private static void Main(string[] args)
    {
        IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        int port = 10001;

        Server server = new Server(iPAddress, port);
        server.Start();
    }
}