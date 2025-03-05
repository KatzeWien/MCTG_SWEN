using NUnit.Framework;
using Npgsql;
using System.Data.Common;
using MonsterTrading.DB;

[TestFixture]
public class UserDBTests
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
    public void CheckCoins_ShouldReturnTrue_WhenCoinsAreSufficient()
    {
        // Arrange
        string username = "testuser1";
        var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO users (username, coins) VALUES (@username, @coins);";
        command.Parameters.Add(new NpgsqlParameter("@username", username));
        command.Parameters.Add(new NpgsqlParameter("@coins", 10)); // Ausreichende Coins
        command.ExecuteNonQuery();

        // Act
        bool result = _userDB.CheckCoins(username);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CheckCoins_ShouldReturnFalse_WhenCoinsAreInsufficient()
    {
        // Arrange
        string username = "testuser2";
        var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO users (username, coins) VALUES (@username, @coins);";
        command.Parameters.Add(new NpgsqlParameter("@username", username));
        command.Parameters.Add(new NpgsqlParameter("@coins", 3)); // Nicht ausreichende Coins
        command.ExecuteNonQuery();

        // Act
        bool result = _userDB.CheckCoins(username);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void CheckPasswordHash()
    {
        string password = "testpassword";
        string hashedPassword = _userDB.HashPassword(password);
        Assert.AreNotEqual(hashedPassword, password);
    }
}