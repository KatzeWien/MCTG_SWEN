using NUnit.Framework;
using Npgsql;
using System;

[TestFixture]
public class DatabaseConnectionTests
{
    private string _connectionString;

    [SetUp]
    public void Setup()
    {
        _connectionString = "Host=localhost;Username=if23b258;Password=123456;Database=testdb";
    }

    [Test]
    public void CanOpenConnectionToDatabase()
    {
        // Arrange
        using var connection = new NpgsqlConnection(_connectionString);

        // Act
        try
        {
            connection.Open();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to open connection: {ex.Message}");
        }

        // Assert
        Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);
    }
}