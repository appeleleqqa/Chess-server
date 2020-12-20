using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Server.Listen();
                Database.Open();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
