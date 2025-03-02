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
    public class PackagesDB
    {
        private DBAccess dBAccess;
        private ServerResponse response;
        public PackagesDB()
        {
            dBAccess = new DBAccess();
            response = new ServerResponse();
        }

        public async Task CreatePackage(string data, StreamWriter writer)
        {
            using (var connection = this.dBAccess.Connect())
            {
                connection.Open();
                var packageData = JsonSerializer.Deserialize<List<Cards>>(data);
                AddCard(packageData);
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
    }
}
