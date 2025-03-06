using MonsterTrading.Server;
using System.Net;
using System.Net.Sockets;

internal class Program
{
    static async Task Main(string[] args)
    {
        IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
        int port = 10001;

        Server server = new Server(iPAddress, port);
        await server.Start();
    }
}