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
    internal class StackDeckTests
    {
        private DbConnection connection;

        private StackDeckDB stackDeckDB;

        [SetUp]
        public void Setup()
        {
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
            CREATE TABLE IF NOT EXISTS cards (
            id VARCHAR(250) PRIMARY KEY,
            name VARCHAR(250),
            damage DOUBLE PRECISION,
            element VARCHAR(50),
            cardtype VARCHAR(50)
            );
            CREATE TABLE IF NOT EXISTS stacks (
            id SERIAL PRIMARY KEY,
            userid VARCHAR(100) REFERENCES users(username) ON DELETE CASCADE,
            cardid VARCHAR(250) REFERENCES cards(id) ON DELETE CASCADE
            );
        ";
            command.ExecuteNonQuery();
            stackDeckDB = new StackDeckDB();
            stackDeckDB.dBAccess.connString = conn;
        }
        [TearDown]
        public async Task TearDown()
        {
            var command = connection.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS users CASCADE;";
            await command.ExecuteNonQueryAsync();
            command.CommandText = "DROP TABLE IF EXISTS cards CASCADE;";
            await command.ExecuteNonQueryAsync();
            command.CommandText = "DROP TABLE IF EXISTS stacks CASCADE;";
            await command.ExecuteNonQueryAsync();

            connection.Close();
        }

        public async Task CreateUser(string username)
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO users (username) VALUES (@username);";
            command.Parameters.Add(new NpgsqlParameter("@username", username));
            await command.ExecuteNonQueryAsync();
        }

        public async Task CreateCard(string cardid)
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO cards (id) VALUES (@cardid);";
            command.Parameters.Add(new NpgsqlParameter("@cardid", cardid));
            await command.ExecuteNonQueryAsync();
        }

        public async Task CreateStack(string cardid, string username)
        {
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO stacks (userid, cardid) VALUES (@userid, @cardid);";
            command.Parameters.Add(new NpgsqlParameter("@userid", username));
            command.Parameters.Add(new NpgsqlParameter("@cardid", cardid));
            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> GetStackCount(string username)
        {
            int count = 0;
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM stacks where userid = @userid;";
            command.Parameters.Add(new NpgsqlParameter("@userid", username));
            var reader = await command.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                count = reader.GetInt32(0);
            }
            reader.Close();
            return count;
        }
        [Test]
        public async Task DeleteCardFromStackAfterLossTest()
        {
            string username = "user1";
            string card1 = "123456";
            string card2 = "234567";
            await CreateUser(username);
            await CreateCard(card1);
            await CreateCard(card2);
            await CreateStack(card1, username);
            await CreateStack(card2, username);
            int CountBeforeDelete = await GetStackCount(username);
            await this.stackDeckDB.DeleteFromStack(card1, username);
            int CountAfterDelete = await GetStackCount(username);
            Assert.IsTrue(CountAfterDelete == CountBeforeDelete - 1 );
        }

        [Test]
        public async Task AddCardFromStackAfterWinTest()
        {
            string username = "user1";
            string card1 = "123456";
            string card2 = "234567";
            await CreateUser(username);
            await CreateCard(card1);
            await CreateCard(card2);
            await CreateStack(card1, username);
            int CountBeforeAdd = await GetStackCount(username);
            await this.stackDeckDB.AddWinnerCard(card2, username);
            int CountAfterAdd = await GetStackCount(username);
            Assert.IsTrue(CountAfterAdd == CountBeforeAdd + 1);
        }
    }
}
