using MonsterTrading.Models;
using MonsterTrading.Server;
using Npgsql;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTrading.DB
{
    public class StackDeckDB
    {
        public DBAccess dBAccess;
        private ServerResponse response;
        public StackDeckDB()
        {
            this.dBAccess = new DBAccess();
            this.response = new ServerResponse();
        }
        public async Task CreateDeck(string name, StreamWriter writer, List<string> cards)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    int affectedRows = 0;
                    foreach (var card in cards)
                    {
                        string statement = "INSERT INTO decks (userid, cardid) VALUES (@userid, @cardid);";
                        await using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        command.Parameters.AddWithValue("cardid", card);
                        affectedRows = affectedRows + await command.ExecuteNonQueryAsync();
                        if (affectedRows == 4)
                        {
                            await response.WriteResponse(writer, 200, "new deck created");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public async Task UpdateDeck(string name, StreamWriter writer, List<string> data)
        {
            using (var connection = await dBAccess.Connect())
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
        public async Task CheckDeckSize(string name, string data, StreamWriter writer)
        {
            using (var connection = await dBAccess.Connect())
            {
                var cards = JsonSerializer.Deserialize<List<string>>(data);
                if (cards.Count != 4)
                {
                    await response.WriteResponse(writer, 400, "only 3 cards");
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
        public async Task ShowDeck(string name, StreamWriter writer)
        {
            if (name == null)
            {
                await response.WriteResponse(writer, 409, "unauthorized");
            }
            else
            {
                await using (var connection = await this.dBAccess.Connect())
                {
                    connection.Open();
                    try
                    {
                        string statement = "SELECT c.name FROM decks d JOIN cards c ON c.id = d.cardid WHERE d.userid = @userid;";
                        await using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        var reader = await command.ExecuteReaderAsync();
                        List<string> cards = new List<string>();
                        while (await reader.ReadAsync())
                        {
                            string card = reader.GetString(0);
                            cards.Add(card);
                        }
                        string listOfCards = string.Join(", ", cards);
                        if (listOfCards == "")
                        {
                            await response.WriteResponse(writer, 200, "list is empty");
                        }
                        else
                        {
                            await response.WriteResponse(writer, 201, listOfCards);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task AddCardstoStack(string name, List<string> cards, StreamWriter writer)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    int affectedrows = 0;
                    foreach (var card in cards)
                    {
                        string statement = "INSERT INTO stacks (userid, cardid) VALUES (@userid, @cardid);";
                        await using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        command.Parameters.AddWithValue("cardid", card);
                        affectedrows = affectedrows + await command.ExecuteNonQueryAsync();
                        if (affectedrows == 5)
                        {
                            await response.WriteResponse(writer, 201, "package added successfully");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task DeleteFromStack(string cardsID, string name)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "DELETE FROM stacks WHERE userid = @userid AND cardid = @cardid;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                    command.Parameters.AddWithValue("cardid", cardsID);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task AddWinnerCard(string cardsid, string name)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "INSERT INTO stacks (userid, cardid) VALUES (@userid, @cardid);";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                    command.Parameters.AddWithValue("cardid", cardsid);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
