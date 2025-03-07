using MonsterTrading.Models;
using MonsterTrading.Server;
using Newtonsoft.Json.Linq;
using Npgsql;
using Npgsql.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MonsterTrading.DB
{
    public class TradesDB
    {
        public DBAccess dBAccess;
        private ServerResponse response;
        public TradesDB()
        {
            this.dBAccess = new DBAccess();
            this.response = new ServerResponse();
        }

        public async Task GetTrades(string name, StreamWriter writer)
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
                        string statement = "SELECT * FROM trades";
                        await using var command = new NpgsqlCommand(statement, connection);
                        var reader = await command.ExecuteReaderAsync();
                        List<string> trades = new List<string>();
                        while (await reader.ReadAsync())
                        {
                            string tradeid = reader.GetString(0);
                            trades.Add(tradeid);
                            string cardid = reader.GetString(2);
                            trades.Add(cardid);
                            string cardType = reader.GetString(3);
                            trades.Add(cardType);
                            double damage = reader.GetDouble(4);
                            trades.Add(damage.ToString());
                        }
                        string listOfCards = string.Join(", ", trades);
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

        public async Task CreateTrade(string data, string token, StreamWriter writer)
        {
            if (token == null)
            {
                await response.WriteResponse(writer, 409, "unauthorized");
            }
            else
            {
                await using (var connection = await dBAccess.Connect())
                {
                    connection.Open();
                    try
                    {
                        var tradeData = JsonSerializer.Deserialize<Trades>(data);
                        string statement = "INSERT INTO trades (id, userid, cardid, type, damage) VALUES (@id, @userid, @cardid, @type, @damage);";
                        await using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("id", tradeData.Id);
                        command.Parameters.AddWithValue("userid", token.Split('-')[0].Trim());
                        command.Parameters.AddWithValue("cardid", tradeData.CardToTrade);
                        command.Parameters.AddWithValue("type", tradeData.Type);
                        command.Parameters.AddWithValue("damage", tradeData.MinimumDamage);
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        if (affectedRows != 0)
                        {
                            await response.WriteResponse(writer, 201, "trade got added");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        await response.WriteResponse(writer, 409, "failure during add trade");
                    }
                }
            }
        }

        public async Task DeleteTrade(string tradeid, string usertoken, StreamWriter writer)
        {
            if (usertoken == null)
            {
                await response.WriteResponse(writer, 409, "unauthorized");
            }
            else
            {
                await using (var connection = await dBAccess.Connect())
                {
                    connection.Open();
                    try
                    {
                        string statement = "DELETE FROM trades WHERE userid = @userid AND id = @id;";
                        await using var command = new NpgsqlCommand(statement, connection);
                        command.Parameters.AddWithValue("userid", usertoken.Split('-')[0]);
                        command.Parameters.AddWithValue("id", tradeid);
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        if (affectedRows != 0)
                        {
                            await response.WriteResponse(writer, 201, "trade got deleted");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        await response.WriteResponse(writer, 400, "something went wrong");
                    }
                }
            }
        }

        public async Task StartTrade(string tradeid, string userToken, StreamWriter writer)
        {
            if (userToken == null)
            {
                await response.WriteResponse(writer, 409, "unauthorized");
            }
            else
            {
                if(await CheckUsersForTrade(tradeid, userToken) == false)
                {
                    await response.WriteResponse(writer, 409, "authorized. no trading with yourself");
                }
                else
                {
                    await response.WriteResponse(writer, 400, "not implemented yet");
                }
            }
        }
        
        public async Task<bool> CheckUsersForTrade(string tradeid, string usertoken)
        {
            await using (var connection = await dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    string useridDB = null;
                    string username = usertoken.Split('-')[0].Trim();
                    string statement = "SELECT userid FROM trades WHERE id = @tradeid;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("tradeid", tradeid);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        useridDB = reader.GetString(0).Trim();
                    }
                    reader.Close();
                    if(username == useridDB)
                    {
                        return false;
                        break;
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
    }
}
