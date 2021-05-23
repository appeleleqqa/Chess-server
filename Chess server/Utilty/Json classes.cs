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
        public string Username { get; set; }
    }

    public class LobbyListMsg
    {
        public int Code { get; set; }
        public List<string> Lobbies { get; set; }
    }

    public class MovesListMsg
    {
        public int Code { get; set; }
        public Dictionary<string, List<string>> Moves { get; set; }
    }
    
    public class MoveMsg
    {
        public int Code { get; set; }
        public string Move;
    }
}
