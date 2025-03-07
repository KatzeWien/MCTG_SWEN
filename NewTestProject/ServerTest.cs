﻿using MonsterTrading.DB;
using MonsterTrading.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NewTestProject
{
    internal class ServerTest
    {
        private Server server;
        private DbConnection connection;

        [SetUp]
        public void Setup()
        {
            string conn = "Host=localhost;Username=if23b258;Password=123456;Database=testdb";
            connection = new NpgsqlConnection(conn);
            connection.Open();
            this.server = new Server(IPAddress.Parse("127.0.0.1"), 10001); 
            this.server.dbAccess.connString = conn;
        }

        [TearDown]
        public void Teardown()
        {
            this.server.dbAccess.DropAllTable();
            connection.Close();
        }

        [Test]
        public async Task RunningAServer()
        {
            TcpListener httpServer = new TcpListener(IPAddress.Parse("127.0.0.1"), 10001);
            var serverTask = Task.Run(() => server.Start());
            await Task.Delay(100);
            using (var client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", 10001);
                Assert.IsTrue(client.Connected, "Der Server sollte Verbindungen akzeptieren.");
            }
            httpServer.Stop();
        }
    }
}
