using MonsterTrading.Server;
using Npgsql;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using MonsterTrading.Models;

namespace MonsterTrading.DB
{
    public class UserDB
    {
        public DBAccess dBAccess;
        private ServerResponse response;
        private PackagesAndCardsDB packageDB;
        private StackDeckDB stackdeckDB;
        private PackagesAndCardsDB cardsDB;
        public UserDB()
        {
            this.dBAccess = new DBAccess();
            this.response = new ServerResponse();
            this.packageDB = new PackagesAndCardsDB();
            this.stackdeckDB = new StackDeckDB();
            this.cardsDB = new PackagesAndCardsDB();
        }

        /*public void ShowAllUser()
        {
            using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                string statement = "SELECT * FROM users;";
                using var command = new NpgsqlCommand(statement, connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string username = reader.GetString(0);
                    //string password = reader.GetString(1);
                    int coins = reader.GetInt32(2);
                    int elo = reader.GetInt32(3);
                    int wins = reader.GetInt32(4);
                    int losses = reader.GetInt32(5);
                    Console.WriteLine($"{username} {coins} {elo} {wins} {losses}");
                }
            }
        }*/

        public async Task CreateUser(string data, StreamWriter writer)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                var userData = JsonSerializer.Deserialize<User>(data);
                try
                {
                    string statement = "INSERT INTO users (username, password, coins, elo, wins, losses) VALUES (@username, @password, @coins, @elo, @wins, @losses);";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", userData.Username);
                    command.Parameters.AddWithValue("password", HashPassword(userData.Password));
                    command.Parameters.AddWithValue("coins", 20);
                    command.Parameters.AddWithValue("elo", 100);
                    command.Parameters.AddWithValue("wins", 0);
                    command.Parameters.AddWithValue("losses", 0);
                    int affectedRows = await command.ExecuteNonQueryAsync();
                    if (affectedRows != 0)
                    {
                        await response.WriteResponse(writer, 201, "user got added");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await response.WriteResponse(writer, 409, "failure during add user");
                }
            }
        }

        public string HashPassword(string password)
        {
            using (SHA256 sHA256 = SHA256.Create())
            {
                byte[] hash = sHA256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        public async Task ShowSpecificUser(string name, StreamWriter writer, string token)
        {
            if (token.Contains(name))
            {
                await using (var connection = await dBAccess.Connect())
                {
                    connection.Open();
                    string statement = "SELECT * FROM users WHERE username = @username;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", name);
                    await using (var reader = await command.ExecuteReaderAsync())
                    { 
                        if (reader.HasRows == false)
                        {
                            await response.WriteResponse(writer, 404, "User not found");
                        }
                        else
                        {
                            while (await reader.ReadAsync())
                            {
                                string username = reader.GetString(0);
                                int coins = reader.GetInt32(2);
                                int elo = reader.GetInt32(3);
                                int wins = reader.GetInt32(4);
                                int losses = reader.GetInt32(5);
                                string bio = reader.GetString(6);
                                string image = reader.GetString(7);
                                string givenname = reader.GetString(8);
                                await response.WriteResponse(writer, 201, $"{username} {coins} {elo} {wins} {losses} {bio} {image} {givenname}");
                            }
                        }
                    }
                }
            }
            else
            {
                await response.WriteResponse(writer, 401, "unauthorized");
            }
        }

        public async Task LoginUser(string data, StreamWriter writer)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                var userData = JsonSerializer.Deserialize<User>(data);
                try
                {
                    string statement = "SELECT password FROM users WHERE username = @username;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", userData.Username);
                    var reader = await command.ExecuteScalarAsync();
                    string hashedpassword = HashPassword(userData.Password);
                    if (reader != null)
                    {
                        string password = reader.ToString();
                        if (hashedpassword == password)
                        {
                            string token = $"{userData.Username}-mtcgtoken";
                            await response.WriteResponse(writer, 200, $"Login Suceessfull {token}");
                        }
                        else
                        {
                            await response.WriteResponse(writer, 401, "Unauthorized");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await response.WriteResponse(writer, 400, "something went wrong");
                }
            }
        }

        public async Task BuyPackage(string name, StreamWriter writer)
        {
            bool canBuy = await CheckCoins(name);
            bool packagesAvailable = await packageDB.CheckPackagesCount();
            if (canBuy == true && packagesAvailable == true)
            {
                using (var connection = await dBAccess.Connect())
                {
                    connection.Open();
                    try
                    {
                        string statement = "SELECT * FROM packages ORDER BY RANDOM() LIMIT 1;";
                        using var command = new NpgsqlCommand(statement, connection);
                        var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            long packageid = reader.GetInt64(0);
                            var card1 = reader.GetString(1);
                            var card2 = reader.GetString(2);
                            var card3 = reader.GetString(3);
                            var card4 = reader.GetString(4);
                            var card5 = reader.GetString(5);
                            List<string> cards = new List<string> { card1, card2, card3, card4, card5 };
                            await this.stackdeckDB.AddCardstoStack(name, cards, writer);
                            await this.packageDB.DeletePackage(packageid);
                        }
                        await DecreaseCoins(name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                if (canBuy == false)
                {
                    await response.WriteResponse(writer, 400, "not enough money");
                }
                else if (packagesAvailable == false)
                {
                    await response.WriteResponse(writer, 400, "no packages available");
                }
            }
        }

        public async Task<bool> CheckCoins(string name)
        {
            using (var connection = await dBAccess.Connect())
            {
                name = name.Split('-')[0];
                connection.Open();
                try
                {
                    string statement = "SELECT coins FROM users WHERE username = @username;";
                    using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", name);
                    var reader = command.ExecuteScalar();
                    if (Convert.ToInt32(reader) < 5)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        public async Task DecreaseCoins(string name)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "UPDATE users SET coins = coins - 5 WHERE username = @username;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", name = name.Split('-')[0]);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                { Console.WriteLine(ex.Message); }
            }
        }

        public async Task UpdateUser(string name, StreamWriter writer, string data, string token)
        {
            if (token.Contains(name))
            {
                await using (var connection = await dBAccess.Connect())
                {
                    connection.Open();
                    var userData = JsonSerializer.Deserialize<User>(data);
                    try
                    {
                        string statement = "UPDATE users SET bio = @bio, image = @image, name = @name WHERE username = @userid;";
                        await using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("bio", userData.Bio);
                        command.Parameters.AddWithValue("image", userData.Image);
                        command.Parameters.AddWithValue("name", userData.Name);
                        command.Parameters.AddWithValue("userid", name);
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        if (affectedRows != 0)
                        {
                            await response.WriteResponse(writer, 201, "user got updated");
                        }
                    }
                    catch(Exception ex)
                    { 
                        Console.WriteLine(ex.Message);
                        await response.WriteResponse(writer, 409, "failure during add user");
                    }
                }
            }
            else
            {
                await response.WriteResponse(writer, 409, "unauthorized");
            }
        }
        public async Task LosserOfBattle(string user, Cards card)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "UPDATE users SET losses = losses + 1, elo = elo - 5 WHERE username = @userid;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("userid", user = user.Split('-')[0]);
                    await command.ExecuteNonQueryAsync();
                    await stackdeckDB.DeleteFromStack(card, user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task WinnerOfBattle(string user, Cards card)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    //Special Feature Winner get one coin
                    string statement = "UPDATE users SET wins = wins + 1, elo = elo + 3, coins = coins + 1 WHERE username = @userid;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("userid", user = user.Split('-')[0]);
                    await command.ExecuteNonQueryAsync();
                    await stackdeckDB.AddWinnerCard(card, user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } 
        }
    }
}
