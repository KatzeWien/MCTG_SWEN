using MonsterTrading.Models;
using MonsterTrading.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static MonsterTrading.Models.Cards;

namespace MonsterTrading.DB
{
    public class PackagesAndCardsDB
    {
        public DBAccess dBAccess;
        private ServerResponse response;
        public PackagesAndCardsDB()
        {
            dBAccess = new DBAccess();
            response = new ServerResponse();
        }

        public async Task CreatePackage(string data, StreamWriter writer, string token)
        {
            if (token.Contains("admin"))
            {
                await using (var connection = await this.dBAccess.Connect())
                {
                    connection.Open();
                    var packageData = JsonSerializer.Deserialize<List<Cards>>(data);
                    await AddCard(packageData);
                    try
                    {
                        string statement = "INSERT INTO packages (firstcard, secondcard, thirdcard, forthcard, fifthcard) VALUES (@firstcard, @secondcard, @thirdcard, @forthcard, @fifthcard);";
                        using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("firstcard", packageData[0].Id);
                        command.Parameters.AddWithValue("secondcard", packageData[1].Id);
                        command.Parameters.AddWithValue("thirdcard", packageData[2].Id);
                        command.Parameters.AddWithValue("forthcard", packageData[3].Id);
                        command.Parameters.AddWithValue("fifthcard", packageData[4].Id);
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        if (affectedRows != 0)
                        {
                            await response.WriteResponse(writer, 201, "package got added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                await response.WriteResponse(writer, 409, "unauthorized");
            }
        }
        public async Task AddCard(List<Cards> cards)
        {
            await using (var connection = await this.dBAccess.Connect())
            {
                connection.Open();
                foreach (var card in cards)
                {
                    if (card.Name.StartsWith("Water", StringComparison.OrdinalIgnoreCase))
                    {
                        card.Element = Cards.Elements.water;
                    }
                    else if (card.Name.StartsWith("Fire", StringComparison.OrdinalIgnoreCase))
                    {
                        card.Element = Cards.Elements.fire;
                    }
                    else
                    {
                        card.Element = Cards.Elements.normal;
                    }
                    if (card.Name.EndsWith("spell", StringComparison.OrdinalIgnoreCase))
                    {
                        card.CardType = Cards.CardTypes.spell;
                    }
                    else
                    {
                        card.CardType = Cards.CardTypes.monster;
                    }
                    try
                    {
                        string statement = "INSERT INTO cards (id, name, damage, element, cardtype) VALUES (@id, @name, @damage, @element, @cardtype);";
                        using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("id", card.Id);
                        command.Parameters.AddWithValue("name", card.Name);
                        command.Parameters.AddWithValue("damage", card.Damage);
                        command.Parameters.AddWithValue("element", card.Element.ToString());
                        command.Parameters.AddWithValue("cardtype", card.CardType.ToString());
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ShowAllCards(string name, StreamWriter writer)
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
                        string statement = "SELECT c.name FROM stacks s JOIN cards c ON c.id = s.cardid WHERE s.userid = @userid;";
                        using var command = new NpgsqlCommand( statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        var reader = await command.ExecuteReaderAsync();
                        List<string> cards = new List<string>();
                        while (reader.Read())
                        {
                            string card = reader.GetString(0);
                            cards.Add(card);
                        }
                        string listOfCards = string.Join(", ", cards);
                        await response.WriteResponse(writer, 201, listOfCards);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task DeletePackage(long packageid)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "DELETE FROM packages WHERE id = @id;";
                    using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("id", packageid);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                { Console.WriteLine(ex.Message); }
            }
        }

        public async Task<bool> CheckPackagesCount()
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "SELECT COUNT(*) FROM packages;";
                    await using var command = new NpgsqlCommand( statement, connection);
                    long rowCount = (long) await command.ExecuteScalarAsync();
                    if (rowCount > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine(ex.Message); 
                    return false;
                }
            }
        }

        public async Task<List<Cards>> PickRandomCard(string user)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    List<Cards> cards = new List<Cards>();
                    string statement = "SELECT c.id, c.name, c.damage, c.element, c.cardtype FROM decks d JOIN cards c ON c.id = d.cardid WHERE d.userid = @userid ORDER BY RANDOM() LIMIT 1;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("userid", user.Split('-')[0]);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        string elementString = reader.GetString(3);
                        string cardString = reader.GetString(4);
                        var item = new Cards
                        (
                            reader.GetString(0),
                            reader.GetString(1),
                            reader.GetDouble(2),
                            Enum.TryParse(elementString, true, out Elements element) ? element : default,
                            Enum.TryParse(cardString, true, out CardTypes cardTypes) ? cardTypes : default
                        );
                        cards.Add(item);
                    }
                    return cards;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }
    }
}
