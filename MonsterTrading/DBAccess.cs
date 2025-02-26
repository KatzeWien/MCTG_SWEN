using Npgsql;
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
            this.connString = "Host=localhost;Username=if23b258;Password=123456;Database=postgres";
        }

        public void Connect()
        {
            using var conn = new NpgsqlConnection(this.connString);
            try
            {
                conn.Open();
                Console.WriteLine("DB Connection successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
