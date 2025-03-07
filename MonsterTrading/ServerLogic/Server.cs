using MonsterTrading.BuisnessLogic;
using MonsterTrading.DB;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MonsterTrading.Server
{
    public class Server
    {
        private TcpListener httpServer;
        public DBAccess dbAccess;
        private UserDB userDB;
        private PackagesAndCardsDB packageDB;
        private StackDeckDB stackdeckDB;
        private ScoreStatsDB scoreStatsDB;
        private Battles battles;
        private TradesDB tradesDB;
        private string? path;
        private string? method;
        private string? body;
        private string? userToken;
        private ServerResponse response;
        private ConcurrentQueue<string> queue;

        public Server(IPAddress address, int port)
        {
            this.httpServer = new TcpListener(address, port);
            this.dbAccess = new DBAccess();
            this.userDB = new UserDB();
            this.packageDB = new PackagesAndCardsDB();
            this.response = new ServerResponse();
            this.stackdeckDB = new StackDeckDB();
            this.scoreStatsDB = new ScoreStatsDB();
            this.battles = new Battles();
            this.queue = new ConcurrentQueue<string>();
            this.tradesDB = new TradesDB();
        }

        public async Task Start()
        {
            //activate DropAllTable when you want to test the curl. For Persistence of DB leave in enabled
            //await dbAccess.DropAllTable();
            await dbAccess.CreateAllTables();
            httpServer.Start();
            while (true)
            {
                var clientSocket = await httpServer.AcceptTcpClientAsync();
                _ = Task.Run(() => GetEndpoint(clientSocket));
            }
        }

        public async Task GetEndpoint(TcpClient clientSocket)
        {
            using var reader = new StreamReader(clientSocket.GetStream());
            using var writer = new StreamWriter(clientSocket.GetStream());
            this.path = null;
            this.method = null;
            this.body = null;
            this.userToken = null;
            AnalyseRequest(reader);
            string[] splitpath = path.Split("/");
            if (path == "/users" || splitpath[1] == "users")
            {
                if (method == "GET")
                {
                    await userDB.ShowSpecificUser(splitpath[2], writer, userToken);
                }
                else if (method == "POST")
                {
                    await userDB.CreateUser(body, writer);
                }
                else if(method == "PUT")
                {
                    await userDB.UpdateUser(splitpath[2], writer, body, userToken);
                }
                else if(method == "DELETE")
                {
                    //Implementation for Delete
                }
            }
            else if (path == "/sessions" || splitpath[1] == "sessions")
            {
                if (method == "GET")
                {
                    //Get Specification
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    if (body != null)
                    {
                        await userDB.LoginUser(body, writer);
                    }
                }
            }
            else if (path == "/packages" || splitpath[1] == "packages")
            {
                if (method == "GET")
                {
                    await response.WriteResponse(writer, 400, "not implementet");
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    await packageDB.CreatePackage(body, writer, this.userToken);
                }
            }
            else if (path == "/transactions" || splitpath[1] == "transactions")
            {
                if (method == "GET")
                {
                    await response.WriteResponse(writer, 400, "not implementet");
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    await userDB.BuyPackage(this.userToken, writer);
                }
            }
            else if (path == "/cards" || splitpath[1] == "cards")
            {
                if (method == "GET")
                {
                    await packageDB.ShowAllCards(this.userToken, writer);
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    await response.WriteResponse(writer, 400, "not implementet");
                }
            }
            else if (path == "/deck" || splitpath[1] == "deck" || splitpath[1].StartsWith("deck"))
            {
                if (method == "GET")
                {
                    await stackdeckDB.ShowDeck(this.userToken, writer);
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    await stackdeckDB.CheckDeckSize(this.userToken, this.body, writer);
                }
            }
            else if (path == "/scoreboard" || splitpath[1] == "scoreboard")
            {
                if (method == "GET")
                {
                    await this.scoreStatsDB.GetAllElos(writer);
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    await response.WriteResponse(writer, 400, "not implementet");
                }
            }
            else if (path == "/battles" || splitpath[1] == "battles")
            {
                if (method == "GET")
                {
                    await packageDB.PickRandomCard(userToken);
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    bool isComplete = false;
                    this.queue.Enqueue(userToken);
                    string user1 = null;
                    string user2 = null;
                    while (!isComplete)
                    {
                        lock (this.queue)
                        {
                            if (this.queue.Count >= 2)
                            {
                                this.queue.TryDequeue(out user1);
                                this.queue.TryDequeue(out user2);
                                isComplete = true; // Markiere, dass die Verarbeitung fertig ist
                            }
                        }
                        if (!isComplete)
                        {
                            await Task.Delay(100); // Asynchron warten
                        }
                        if (queue.Count == 0)
                        {
                            await battles.StartBattle(user1, user2, writer);
                            return;
                        }
                    }
                }
            }
            else if (path == "/stats" || splitpath[1] == "stats")
            {
                if (method == "GET")
                {
                    await this.scoreStatsDB.GetUserStats(this.userToken, writer);   
                }
                else if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    //Implementation 
                }
            }
            else if (path == "/tradings" || splitpath[1] == "tradings")
            {
                if (method == "GET")
                {
                    await tradesDB.GetTrades(this.userToken, writer);
                }
                else if (method == "PUT" || method == "DELETE")
                {
                    await tradesDB.DeleteTrade(splitpath[2], userToken, writer);
                }
                else if (method == "POST")
                {
                    if (splitpath.Length == 2)
                    {
                        await tradesDB.CreateTrade(body, userToken, writer);
                    }
                    else
                    {
                        await tradesDB.CheckUsersForTrade(splitpath[2], userToken);
                    }
                }
            }
        }

        public void AnalyseRequest(StreamReader reader)
        {
            // ----- 1. Read the HTTP-Request -----
            string? line;

            // 1.1 first line in HTTP contains the method, path and HTTP version
            line = reader.ReadLine();
            if (line != null)
            {
                var partsOfFirstLine = line.Split(" ");
                method = partsOfFirstLine[0];
                path = partsOfFirstLine[1];
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
                if (parts[0].Trim() == "Authorization")
                {
                    this.userToken = parts[1].Split(" ")[2].Trim();
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
                body = data.ToString();
            }
        }
    }
}