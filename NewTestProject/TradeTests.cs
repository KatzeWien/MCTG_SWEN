using MonsterTrading.DB;
using MonsterTrading.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewTestProject
{
    internal class TradeTests
    {
        private TradesDB tradesDB;
        private DbConnection connection;
        [SetUp]
        public void Setup()
        {
            string conn = "Host=localhost;Username=if23b258;Password=123456;Database=testdb";
            connection = new NpgsqlConnection(conn);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                username TEXT PRIMARY KEY
            );
            CREATE TABLE IF NOT EXISTS cards (
                id TEXT PRIMARY KEY
            );
            CREATE TABLE IF NOT EXISTS trades (
                id VARCHAR(250) PRIMARY KEY,
                userid VARCHAR(100) REFERENCES users(username) ON DELETE CASCADE,
                cardid VARCHAR(250) REFERENCES cards(id) ON DELETE CASCADE,
                type VARCHAR(50),
                damage DOUBLE PRECISION
            );
            INSERT INTO users (username) VALUES ('testuser');
            INSERT INTO cards (id) VALUES ('123456');
        ";
            command.ExecuteNonQuery();
            tradesDB = new TradesDB();
            tradesDB.dBAccess.connString = conn;
        }
        [TearDown]
        public void Teardown()
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
            DROP TABLE IF EXISTS trades;
            DROP TABLE IF EXISTS cards;
            DROP TABLE IF EXISTS users;
            ";
            command.ExecuteNonQuery();

            connection.Close();
        }
        [Test]
        public async Task GetEmptyListOfTrades()
        {
            using var memorystream = new MemoryStream();
            using var writer = new StreamWriter(memorystream);
            await this.tradesDB.GetTrades("testuser", writer);
            writer.Flush();
            memorystream.Position = 0;

            using var reader = new StreamReader(memorystream);
            string result = await reader.ReadToEndAsync();

            Assert.IsTrue(result.Contains("200"));
        }

        [Test]
        public async Task AddTradeToDBTest()
        {
            string data = "{\"Id\": \"1\", \"CardToTrade\": \"123456\", \"Type\": \"monster\", \"MinimumDamage\": 15}";
            using var memorystream = new MemoryStream();
            using var writer = new StreamWriter(memorystream);
            await this.tradesDB.CreateTrade(data, "testuser", writer);
            writer.Flush();
            memorystream.Position = 0;

            using var reader = new StreamReader(memorystream);
            string result = await reader.ReadToEndAsync();

            Assert.IsTrue(result.Contains("201"));
        }

        [Test]
        public async Task DeleteTradeFromTradeDB()
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO trades (id, userid, cardid, type, damage) VALUES (@id, @userid, @cardid, @type, @damage);";
            command.Parameters.Add(new NpgsqlParameter("@id", 1));
            command.Parameters.Add(new NpgsqlParameter("@userid", "testuser"));
            command.Parameters.Add(new NpgsqlParameter("@cardid", "123456"));
            command.Parameters.Add(new NpgsqlParameter("@type", "monster"));
            command.Parameters.Add(new NpgsqlParameter("@damage", 15));
            command.ExecuteNonQuery();
            using var memorystream = new MemoryStream();
            using var writer = new StreamWriter(memorystream);
            await tradesDB.DeleteTrade("1", "testuser", writer);
            writer.Flush();
            memorystream.Position = 0;

            using var reader = new StreamReader(memorystream);
            string result = await reader.ReadToEndAsync();
            Assert.IsTrue(result.Contains("201"));
        }
    }
}
