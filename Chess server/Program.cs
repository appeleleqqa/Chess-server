using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_server
{
    class Program
    {
        static void Main()
        {
            try
            {
                Database.Open();
                Server.Listen();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
