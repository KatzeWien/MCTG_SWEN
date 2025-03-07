﻿using NUnit.Framework;
using Npgsql;
using System.Data.Common;
using MonsterTrading.DB;
using System.IO;

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
    public async Task CheckCoinsShouldReturnTrueWhenCoinsAreSufficient()
    {
        // Arrange
        string username = "testuser1";
        var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO users (username, coins) VALUES (@username, @coins);";
        command.Parameters.Add(new NpgsqlParameter("@username", username));
        command.Parameters.Add(new NpgsqlParameter("@coins", 10)); // Ausreichende Coins
        command.ExecuteNonQuery();

        // Act
        bool result = await _userDB.CheckCoins(username);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public async Task CheckCoinsShouldReturnFalseWhenCoinsAreInsufficient()
    {
        // Arrange
        string username = "testuser2";
        var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO users (username, coins) VALUES (@username, @coins);";
        command.Parameters.Add(new NpgsqlParameter("@username", username));
        command.Parameters.Add(new NpgsqlParameter("@coins", 3)); // Nicht ausreichende Coins
        command.ExecuteNonQuery();

        // Act
        bool result = await _userDB.CheckCoins(username);

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

    [Test]
    public async Task CheckLoginWithFalseCred()
    {
        string username = "kienboec";
        string password = _userDB.HashPassword("daniel");
        var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO users (username, password) VALUES (@username, @password);";
        command.Parameters.Add(new NpgsqlParameter("@username", username));
        command.Parameters.Add(new NpgsqlParameter("@password", password));
        command.ExecuteNonQuery();
        string data = "{\"Username\":\"kienboec\", \"Password\":\"falsches\"}";
        using var memorystream = new MemoryStream();
        using var writer = new StreamWriter(memorystream);
        await _userDB.LoginUser(data, writer);
        writer.Flush();
        memorystream.Position = 0;

        using var reader = new StreamReader(memorystream);
        string result = await reader.ReadToEndAsync();

        Assert.IsTrue(result.Contains("Unauthorized"));
    }

    [Test]
    public async Task GetUnauthorizedWithWrongToken()
    {
        string user = "kienboec";
        string token = "admin-token";
        using var memorystream = new MemoryStream();
        using var writer = new StreamWriter(memorystream);
        await _userDB.ShowSpecificUser(user, writer, token);
        writer.Flush();
        memorystream.Position = 0;

        using var reader = new StreamReader(memorystream);
        string result = await reader.ReadToEndAsync();

        Assert.IsTrue(result.Contains("unauthorized"));
    }

    [Test]
    public async Task CreateUserTest()
    {
        string data = "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}";
        using var memorystream = new MemoryStream();
        using var writer = new StreamWriter(memorystream);
        await _userDB.CreateUser(data, writer);
        writer.Flush();
        memorystream.Position = 0;
        using var reader = new StreamReader(memorystream);
        string result = await reader.ReadToEndAsync();
        Assert.IsTrue(result.Contains("user got added"));
    }
}