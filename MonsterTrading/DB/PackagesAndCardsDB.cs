using MonsterTrading.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTrading.DB
{
    public class PackagesAndCardsDB
    {
        private DBAccess dBAccess;
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
                using (var connection = this.dBAccess.Connect())
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
                        int affectedRows = command.ExecuteNonQuery();
                        if (affectedRows != 0)
                        {
                            response.WriteResponse(writer, 201, "package got added");
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
                response.WriteResponse(writer, 409, "unauthorized");
            }
        }
        public async Task AddCard(List<Cards> cards)
        {
            using (var connection = this.dBAccess.Connect())
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
                        command.ExecuteNonQuery();
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
                response.WriteResponse(writer, 409, "unauthorized");
            }
            else
            {
                using (var connection = this.dBAccess.Connect())
                {
                    connection.Open();
                    try
                    {
                        string statement = "SELECT c.name FROM stacks s JOIN cards c ON c.id = s.cardid WHERE s.userid = @userid;";
                        using var command = new NpgsqlCommand( statement, connection);
                        command.Parameters.AddWithValue("userid", name.Split('-')[0]);
                        var reader = command.ExecuteReader();
                        List<string> cards = new List<string>();
                        while (reader.Read())
                        {
                            string card = reader.GetString(0);
                            cards.Add(card);
                        }
                        string listOfCards = string.Join(", ", cards);
                        response.WriteResponse(writer, 201, listOfCards);
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
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "DELETE FROM packages WHERE id = @id;";
                    using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("id", packageid);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                { Console.WriteLine(ex.Message); }
            }
        }

        public bool CheckPackagesCount()
        {
            using (var connection = dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string statement = "SELECT COUNT(*) FROM packages;";
                    using var command = new NpgsqlCommand( statement, connection);
                    long rowCount = (long)command.ExecuteScalar();
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
    }
}
