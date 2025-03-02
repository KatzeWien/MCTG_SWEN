using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTrading.Server
{
    public class ServerResponse
    {
        public void WriteResponse(StreamWriter writer, int statusCode, string message)
        {
            string test = $"HTTP/1.1 {statusCode} - {message}";
            writer.WriteLine(test);
            writer.Flush();
            Console.WriteLine(test);
        }
    }
}
