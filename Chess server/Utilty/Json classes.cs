using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess_server
{
    //json classes are used to serialize and deserialize json strings using the newtonsoft
    public class UserLogin
    {
        public int Code { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LobbyMsg
    {
        public int Code { get; set; }
        public string HostName { get; set; }
    }
}
