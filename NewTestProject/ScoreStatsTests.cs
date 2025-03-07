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
    internal class ScoreStatsTests
    {
        private ScoreStatsDB scoreStatsDB;
        private DbConnection connection;

        [SetUp]
        public void Setup()
        {
            this.scoreStatsDB = new ScoreStatsDB();
            string conn = "Host=localhost;Username=if23b258;Password=123456;Database=testdb";
            connection = new NpgsqlConnection(conn);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS users (
                username TEXT PRIMARY KEY,
                password TEXT,
                coins INTEGER,
                elo INTEGER,
                wins INTEGER,
                losses INTEGER
            );
        ";
            command.ExecuteNonQuery();
        }

        [TearDown]
        public void Teardown()
        {
            var command = connection.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS users;";
            command.ExecuteNonQuery();
            connection.Close();
        }

        [Test]
        public async Task GetAllElosTest()
        {
            string user1 = "user1";
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (username, elo) VALUES (@username, @elo);";
            command.Parameters.Add(new NpgsqlParameter("@username", user1));
            command.Parameters.Add(new NpgsqlParameter("@elo", 50)); 
            command.ExecuteNonQuery();
            string user2 = "user2";
            var command2 = connection.CreateCommand();
            command2.CommandText = "INSERT INTO users (username, elo) VALUES (@username, @elo);";
            command2.Parameters.Add(new NpgsqlParameter("@username", user2));
            command2.Parameters.Add(new NpgsqlParameter("@elo", 50));
            command2.ExecuteNonQuery();
            using var memorystream = new MemoryStream();
            using var writer = new StreamWriter(memorystream);
            await scoreStatsDB.GetAllElos(writer);
            writer.Flush();
            memorystream.Position = 0;

            using var reader = new StreamReader(memorystream);
            string result = await reader.ReadToEndAsync();

            Assert.IsTrue(result.Contains("201"));
        }

        /*[Test]
        public async Task GetUserStatsTest()
        {
            string token = "user1-token";
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (username, elo, wins, losses) VALUES (@username, @elo, @wins, @losses);";
            command.Parameters.Add(new NpgsqlParameter("@username", token.Split('-')[0].Trim()));
            command.Parameters.Add(new NpgsqlParameter("@elo", 50));
            command.Parameters.Add(new NpgsqlParameter("@wins", 10));
            command.Parameters.Add(new NpgsqlParameter("@losses", 8));
            await command.ExecuteNonQueryAsync();
            using var memorystream = new MemoryStream();
            using var writer = new StreamWriter(memorystream);
            await scoreStatsDB.GetUserStats(token, writer);
            writer.Flush();
            memorystream.Position = 0;

            using var reader = new StreamReader(memorystream);
            string result = await reader.ReadToEndAsync();

            Assert.IsTrue(result.Contains("201"));
        }*/
    }
}
