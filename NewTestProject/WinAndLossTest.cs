using MonsterTrading.DB;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTestProject
{
    internal class WinAndLossTest
    {
        private DbConnection _connection;

        private UserDB _userDB;

        [SetUp]
        public void Setup()
        {
            string conn = "Host=localhost;Username=if23b258;Password=123456;Database=testdb";
            _connection = new NpgsqlConnection(conn);
            _connection.Open();

            var command = _connection.CreateCommand();
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



            _userDB = new UserDB(); // Verwende den Standardkonstruktor
            _userDB.dBAccess.connString = conn;
        }

        [TearDown]
        public void Teardown()
        {
            var command = _connection.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS users;";
            command.ExecuteNonQuery();

            _connection.Close();
        }

        [Test]
        public void ChangeStatsWin()
        {
            string username = "user1";
            int elo = 100;
            int wins = 0;
            int losses = 0;
            int coins = 0;
            var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO users (username, elo, wins, losses, coins) VALUES (@username, @elo, @wins, @losses, @coins);";
            command.Parameters.Add(new NpgsqlParameter("@username", username));
            command.Parameters.Add(new NpgsqlParameter("@elo", elo));
            command.Parameters.Add(new NpgsqlParameter("@wins", wins));
            command.Parameters.Add(new NpgsqlParameter("@losses", losses));
            command.Parameters.Add(new NpgsqlParameter("@coins", coins));
            command.ExecuteNonQuery();
            //user1 wins a battle
            var command2 = _connection.CreateCommand();
            command2.CommandText = "UPDATE users SET wins = wins + 1, elo = elo + 3, coins = coins + 1 WHERE username = @username;";
            command2.Parameters.Add(new NpgsqlParameter("@username", username));
            command2.ExecuteNonQuery();
            var command3 = _connection.CreateCommand();
            command3.CommandText = "SELECT username, elo,wins,losses,coins FROM users WHERE username = @username;";
            command3.Parameters.Add(new NpgsqlParameter("@username", username));
            var reader = command3.ExecuteReader();
            while (reader.Read())
            {
                Assert.AreEqual(username, reader.GetString(0));
                Assert.AreEqual(elo + 3, reader.GetInt32(1));
                Assert.AreEqual(wins + 1, reader.GetInt32(2));
                Assert.AreEqual(losses, reader.GetInt32(3));
                Assert.AreEqual(coins + 1, reader.GetInt32(4));
            }
            reader.Close();
        }

        [Test]
        public void ChangeStatsLoss()
        {
            string username = "user1";
            int elo = 100;
            int wins = 0;
            int losses = 0;
            var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO users (username, elo, wins, losses) VALUES (@username, @elo, @wins, @losses);";
            command.Parameters.Add(new NpgsqlParameter("@username", username));
            command.Parameters.Add(new NpgsqlParameter("@elo", elo));
            command.Parameters.Add(new NpgsqlParameter("@wins", wins));
            command.Parameters.Add(new NpgsqlParameter("@losses", losses));
            command.ExecuteNonQuery();
            //user1 wins a battle
            var command2 = _connection.CreateCommand();
            command2.CommandText = "UPDATE users SET losses = losses + 1, elo = elo - 5 WHERE username = @username;";
            command2.Parameters.Add(new NpgsqlParameter("@username", username));
            command2.ExecuteNonQuery();
            var command3 = _connection.CreateCommand();
            command3.CommandText = "SELECT username, elo,wins,losses FROM users WHERE username = @username;";
            command3.Parameters.Add(new NpgsqlParameter("@username", username));
            var reader = command3.ExecuteReader();
            while (reader.Read())
            {
                Assert.AreEqual(username, reader.GetString(0));
                Assert.AreEqual(elo - 5, reader.GetInt32(1));
                Assert.AreEqual(wins, reader.GetInt32(2));
                Assert.AreEqual(losses + 1, reader.GetInt32(3));
            }
            reader.Close();
        }
    }
}
