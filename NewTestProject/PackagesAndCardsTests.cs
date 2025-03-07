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
    internal class PackagesAndCardsTests
    {
        private DbConnection connection;

        private PackagesAndCardsDB packagesAndCards;

        [SetUp]
        public void Setup()
        {
            string conn = "Host=localhost;Username=if23b258;Password=123456;Database=testdb";
            connection = new NpgsqlConnection(conn);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS cards (
            id VARCHAR(250) PRIMARY KEY,
            name VARCHAR(250),
            damage DOUBLE PRECISION,
            element VARCHAR(50),
            cardtype VARCHAR(50)
            );

            CREATE TABLE IF NOT EXISTS packages (
            id SERIAL PRIMARY KEY,
            firstcard VARCHAR(250) REFERENCES cards(id) ON DELETE CASCADE,
            secondcard VARCHAR(250) REFERENCES cards(id) ON DELETE CASCADE,
            thirdcard VARCHAR(250) REFERENCES cards(id) ON DELETE CASCADE,
            forthcard VARCHAR(250) REFERENCES cards(id) ON DELETE CASCADE,
            fifthcard VARCHAR(250) REFERENCES cards(id) ON DELETE CASCADE
            );
        ";
            command.ExecuteNonQuery();
            packagesAndCards = new PackagesAndCardsDB();
            packagesAndCards.dBAccess.connString = conn;
        }
        [TearDown]
        public void TearDown()
        {
            var command = connection.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS packages;";
            command.ExecuteNonQuery();
            command.CommandText = "DROP TABLE IF EXISTS cards;";

            connection.Close();
        }
        [Test]
        public async Task PackagesAvailableTest()
        {
            //bool should be false
            bool available = await this.packagesAndCards.CheckPackagesCount();
            Assert.IsFalse(available);
        }
    }
}
