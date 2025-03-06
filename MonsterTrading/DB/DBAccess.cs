using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrading.DB
{
    public class DBAccess
    {
        public string connString;
        public DBAccess()
        {
            connString = "Host=localhost;Username=if23b258;Password=123456;Database=mtcg";
        }

        public async Task<NpgsqlConnection> Connect()
        {
            using var conn = new NpgsqlConnection(connString);
            try
            {
                await conn.OpenAsync();
                Console.WriteLine("DB Connection successful");
                return new NpgsqlConnection(connString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task DropAllTable()
        {
            try
            {
                string[] sqlDel = await File.ReadAllLinesAsync("C:\\Users\\danie\\Documents\\Fachhochschule\\FHTW\\3. Semester\\C#\\MonsterTrading\\MonsterTrading\\DB\\DropAllTables.txt");
                using (var connection = await Connect())
                {
                    connection.Open();
                    foreach (string sql in sqlDel)
                    {
                        using var command = new NpgsqlCommand(sql, connection);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task CreateAllTables()
        {
            try
            {
                string script = await File.ReadAllTextAsync("C:\\Users\\danie\\Documents\\Fachhochschule\\FHTW\\3. Semester\\C#\\MonsterTrading\\MonsterTrading\\DB\\AddTables.txt");
                using (var connection = await Connect())
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
