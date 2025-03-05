using MonsterTrading.Server;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrading.DB
{
    public class ScoreStatsDB
    {
        private ServerResponse response;
        private DBAccess dBAccess;
        public ScoreStatsDB()
        {
            this.response = new ServerResponse();
            this.dBAccess = new DBAccess();
        }
        public async Task GetUserStats(string token, StreamWriter writer)
        {
            await using (var connection = dBAccess.Connect())
            {
                connection.Open();
                token = token.Split('-')[0];
                try
                {
                    string statement = "SELECT elo, losses, wins FROM users WHERE username = @user;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    command.Parameters.AddWithValue("user", token);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int elo = reader.GetInt32(0);
                        int wins = reader.GetInt32(1);
                        int losses = reader.GetInt32(2);
                        response.WriteResponse(writer, 201, $"Elo: {elo} Wins: {wins} Losses: {losses}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    response.WriteResponse(writer, 400, "something went wrong");
                }
            }
        }

        public async Task GetAllElos(StreamWriter writer)
        {
            await using (var connection = dBAccess.Connect())
            {
                connection.Open();
                try
                {
                    int elo;
                    string users;
                    string result = null;
                    string statement = "SELECT elo, username FROM users ORDER BY elo;";
                    await using var command = new NpgsqlCommand(statement, connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        elo = reader.GetInt32(0);
                        users = reader.GetString(1);
                        result = result + string.Join(", ", $"{users}: {elo} ");
                    }
                    response.WriteResponse(writer, 201, result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    response.WriteResponse(writer, 400, "something went wrong");
                }
            }
        }
    }
}
