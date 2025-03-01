﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrading
{
    public class DBAccess
    {
        private string connString;
        public DBAccess()
        {
            this.connString = "Host=localhost;Username=if23b258;Password=123456;Database=mtcg";
        }

        public NpgsqlConnection Connect()
        {
            using var conn = new NpgsqlConnection(this.connString);
            try
            {
                conn.Open();
                Console.WriteLine("DB Connection successful");
                return new NpgsqlConnection(this.connString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void DropAllTable()
        {
            try
            {
                string[] sqlDel = File.ReadAllLines("C:\\Users\\danie\\Documents\\Fachhochschule\\FHTW\\3. Semester\\C#\\MonsterTrading\\MonsterTrading\\DropAllTables.txt");
                using (var connection = Connect())
                {
                    connection.Open();
                    foreach (string sql in sqlDel)
                    {
                        using var command = new NpgsqlCommand(sql, connection);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void CreateAllTables()
        {
            try
            {
                string script = File.ReadAllText("C:\\Users\\danie\\Documents\\Fachhochschule\\FHTW\\3. Semester\\C#\\MonsterTrading\\MonsterTrading\\AddTables.txt");
                using (var connection = Connect())
                {
                    connection.Open();
                    using var command = new NpgsqlCommand(script, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
