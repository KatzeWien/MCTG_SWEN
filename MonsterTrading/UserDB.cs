﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MonsterTrading
{
    public class UserDB
    {
        private DBAccess dBAccess;
        private ServerResponse response;
        public UserDB()
        {
            this.dBAccess = new DBAccess();
            this.response = new ServerResponse();
        }

        public void ShowAllUser()
        {
            using (var connection = this.dBAccess.Connect())
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
            using (var connection = this.dBAccess.Connect())
            {
                connection.Open();
                var userData = JsonSerializer.Deserialize<User>(data);
                try
                {
                    string statement = "INSERT INTO users (username, password, coins, elo, wins, losses) VALUES (@username, @password, @coins, @elo, @wins, @losses);";
                    using var command = new NpgsqlCommand (statement, connection);
                    command.Parameters.AddWithValue("username", userData.Username);
                    command.Parameters.AddWithValue("password", HashPassword(userData.Password));
                    command.Parameters.AddWithValue("coins", 20);
                    command.Parameters.AddWithValue("elo", 100);
                    command.Parameters.AddWithValue("wins", 0);
                    command.Parameters.AddWithValue("losses", 0);
                    int affectedRows = command.ExecuteNonQuery();
                    if (affectedRows != 0)
                    {
                        this.response.WriteResponse(writer, 201, "user got added");
                        //WriteResponse(writer, 201, "user got added");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    this.response.WriteResponse(writer, 409, "failure during add user");
                }
            }
        }

        public string HashPassword(string password)
        {
            using(SHA256 sHA256 = SHA256.Create())
            {
                byte[] hash = sHA256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        public async Task ShowSpecificUser(string name, StreamWriter writer)
        {
            using (var connection = this.dBAccess.Connect())
            {
                connection.Open();
                string statement = "SELECT * FROM users WHERE username = @username;";
                using var command = new NpgsqlCommand(statement, connection);
                command.Parameters.AddWithValue ("username", name);
                var reader = command.ExecuteReader();
                if (reader.Rows == 0)
                {
                    this.response.WriteResponse(writer, 404, "User not found");
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
                    this.response.WriteResponse(writer, 201, "");
                }
            }
        }

        public async Task LoginUser(string data, StreamWriter writer)
        {
            using (var connection = this.dBAccess.Connect())
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
                    if(reader != null)
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
    }
}
