using MonsterTrading.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTrading.DB
{
    public class UserDB
    {
        public DBAccess dBAccess;
        private ServerResponse response;
        public UserDB()
        {
            dBAccess = new DBAccess();
            response = new ServerResponse();
        }

        public async Task WhichMethod(string method, string data, StreamWriter writer)
        {
            switch (method)
            {
                case "POST":
                    CreateUser(data, writer);
                    break;
                case "PUT":
                    response.WriteResponse(writer, 400, "not implementet");
                    break;
                case "DELETE":
                    response.WriteResponse(writer, 400, "not implementet");
                    break;
            }
        }

        public void ShowAllUser()
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                string statement = "SELECT * FROM users;";
                using var command = new NpgsqlCommand(statement, connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string username = reader.GetString(0);
                    string password = reader.GetString(1);
                    int coins = reader.GetInt32(2);
                    int elo = reader.GetInt32(3);
                    int wins = reader.GetInt32(4);
                    int losses = reader.GetInt32(5);

                    Console.WriteLine($"{username} {password} {coins} {elo} {wins} {losses}");
                }
            }
        }

        public async Task CreateUser(string data, StreamWriter writer)
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                var userData = JsonSerializer.Deserialize<User>(data);
                try
                {
                    string statement = "INSERT INTO users (username, password, coins, elo, wins, losses) VALUES (@username, @password, @coins, @elo, @wins, @losses);";
                    using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", userData.Username);
                    command.Parameters.AddWithValue("password", HashPassword(userData.Password));
                    command.Parameters.AddWithValue("coins", 20);
                    command.Parameters.AddWithValue("elo", 100);
                    command.Parameters.AddWithValue("wins", 0);
                    command.Parameters.AddWithValue("losses", 0);
                    int affectedRows = command.ExecuteNonQuery();
                    if (affectedRows != 0)
                    {
                        response.WriteResponse(writer, 201, "user got added");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    response.WriteResponse(writer, 409, "failure during add user");
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

        public async Task ShowSpecificUser(string name, StreamWriter writer)
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                string statement = "SELECT * FROM users WHERE username = @username;";
                using var command = new NpgsqlCommand(statement, connection);
                command.Parameters.AddWithValue("username", name);
                var reader = command.ExecuteReader();
                if (reader.Rows == 0)
                {
                    response.WriteResponse(writer, 404, "User not found");
                }
                while (reader.Read())
                {
                    string username = reader.GetString(0);
                    string password = reader.GetString(1);
                    int coins = reader.GetInt32(2);
                    int elo = reader.GetInt32(3);
                    int wins = reader.GetInt32(4);
                    int losses = reader.GetInt32(5);

                    Console.WriteLine($"{username} {password} {coins} {elo} {wins} {losses}");
                    response.WriteResponse(writer, 201, "");
                }
            }
        }

        public async Task LoginUser(string data, StreamWriter writer)
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                var userData = JsonSerializer.Deserialize<User>(data);
                try
                {
                    string statement = "SELECT password FROM users WHERE username = @username;";
                    using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", userData.Username);
                    var reader = command.ExecuteScalar();
                    string hashedpassword = HashPassword(userData.Password);
                    if (reader != null)
                    {
                        string password = reader.ToString();
                        if (hashedpassword == password)
                        {
                            string token = $"{userData.Username}-mtcgtoken";
                            response.WriteResponse(writer, 200, $"Login Suceessfull {token}");
                        }
                        else
                        {
                            response.WriteResponse(writer, 401, "Unauthorized");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    response.WriteResponse(writer, 400, "something went wrong");
                }
            }
        }

        public async Task BuyPackage(string name, StreamWriter writer)
        {
            bool canBuy = CheckCoins(name);
            if (canBuy == true)
            {
                using (var connection = dBAccess.Connect())
                {
                    connection.Open();
                    try
                    {
                        string statement = "SELECT * FROM packages ORDER BY RANDOM() LIMIT 1;";
                        using var command = new NpgsqlCommand(statement, connection);
                        command.CommandTimeout = 0;
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            var card1 = reader.GetString(1);
                            var card2 = reader.GetString(2);
                            var card3 = reader.GetString(3);
                            var card4 = reader.GetString(4);
                            var card5 = reader.GetString(5);
                            List<string> cards = new List<string> { card1, card2, card3, card4, card5 };
                            AddCardstoStack(name, cards, writer);
                        }
                        DecreaseCoins(name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                response.WriteResponse(writer, 400, "not enough money");
            }
        }

        public bool CheckCoins(string name)
        {
            using (var connection = dBAccess.Connect())
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
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "UPDATE users SET coins = coins - 5 WHERE username = @username;";
                    using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("username", name = name.Split('-')[0]);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                { Console.WriteLine(ex.Message); }
            }
        }

        public async Task AddCardstoStack(string name, List<string> cards, StreamWriter writer)
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    int affectedrows = 0;
                    foreach (var card in cards)
                    {
                        string statement = "INSERT INTO stacks (userid, cardid) VALUES (@userid, @cardid);";
                        using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        command.Parameters.AddWithValue("cardid", card);
                        affectedrows = affectedrows + command.ExecuteNonQuery();
                        if (affectedrows == 5)
                        {
                            response.WriteResponse(writer, 201, "package added successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task ShowDeck(string name, StreamWriter writer)
        {
            if (name == null)
            {
                response.WriteResponse(writer, 409, "unauthorized");
            }
            else
            {
                using (var connection = this.dBAccess.Connect())
                {
                    connection.Open();
                    try
                    {
                        string statement = "SELECT c.name FROM decks d JOIN cards c ON c.id = d.cardid WHERE d.userid = @userid;";
                        using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        var reader = command.ExecuteReader();
                        List<string> cards = new List<string>();
                        while (reader.Read())
                        {
                            string card = reader.GetString(0);
                            cards.Add(card);
                        }
                        string listOfCards = string.Join(", ", cards);
                        if (listOfCards == "")
                        {
                            response.WriteResponse(writer, 200, "list is empty");
                        }
                        else
                        {
                            response.WriteResponse(writer, 201, listOfCards);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task CheckDeckSize(string name, string data, StreamWriter writer)
        {
            using (var connection = dBAccess.Connect())
            {
                var cards = JsonSerializer.Deserialize<List<string>>(data);
                if (cards.Count != 4)
                {
                    response.WriteResponse(writer, 400, "only 3 cards");
                }
                else
                {
                    connection.Open();
                    try
                    {
                        string statement = "SELECT COUNT(*) FROM decks WHERE userid = @userid;";
                        using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        long rowCount = (long)command.ExecuteScalar();
                        if (rowCount == 0)
                        {
                            await CreateDeck(name, writer, cards);
                        }
                        else
                        {
                            await UpdateDeck(name, writer, cards);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task CreateDeck(string name, StreamWriter writer, List<string> cards)
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    int affectedRows = 0;
                    foreach (var card in cards)
                    {
                        string statement = "INSERT INTO decks (userid, cardid) VALUES (@userid, @cardid);";
                        using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        command.Parameters.AddWithValue("cardid", card);
                        affectedRows = affectedRows + command.ExecuteNonQuery();
                        if (affectedRows == 4)
                        {
                            response.WriteResponse(writer, 200, "new deck created");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task UpdateDeck(string name ,StreamWriter writer, List<string> data)
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "DELETE FROM decks WHERE userid = @userid;";
                    using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                    command.ExecuteNonQuery();
                    await CreateDeck(name, writer, data);
                }
                catch (Exception ex)
                { Console.WriteLine(ex.Message); }
            }
        }
    }
}
