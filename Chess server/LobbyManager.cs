using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Chess_server
{
    //takes care of cenarios that revolve a lobby
    class LobbyManager
    {
        private static Mutex lobbyDictMutex = new Mutex();
        //a dictionaey contaning all the lobbies in this template
        // [host name] = {host, player2}
        static Dictionary<string, Tuple<TcpClient, TcpClient>> lobbies = new Dictionary<string, Tuple<TcpClient, TcpClient>>();

        public static string PlayerNavagation(string username, NetworkStream stream)
        {
            while(true)
            {
                string msg = Server.ReceiveMsg(stream);
                LobbyMsg lby = JsonConvert.DeserializeObject<LobbyMsg>(msg);
                switch (lby.Code)
                {
                    case (int)msgCodes.JoinLobby:
                        msg = ((int)JoinLobby(username, lby.LobbyName)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        if (msg == ((int)msgCodes.LobbyJoined).ToString())
                            return lby.LobbyName;
                        break;
                    case (int)msgCodes.CreateLobby:
                        msg = ((int)msgCodes.LobbyCreated).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        if (msg == ((int)msgCodes.LobbyJoined).ToString())
                            return lby.LobbyName;
                        break;
                }
            }
        }

        public static void OpenLobby(string username)
        {
            lobbyDictMutex.WaitOne();
            lobbies[username] = new Tuple<TcpClient, TcpClient>(LoginManager.users[username], null);
            lobbyDictMutex.ReleaseMutex();
        }

        public static msgCodes JoinLobby(string username, string lobbyname)
        {
            lobbyDictMutex.WaitOne();
            if (lobbies.ContainsKey(lobbyname))
            {
                if (lobbies[lobbyname].Item2 == null)
                {
                    lobbies[lobbyname] = new Tuple<TcpClient, TcpClient>(lobbies[lobbyname].Item1, LoginManager.users[username]);
                    lobbyDictMutex.ReleaseMutex();
                    return msgCodes.LobbyJoined;
                }
                return msgCodes.LobbyFull;
            }
            lobbyDictMutex.ReleaseMutex();
            return msgCodes.LobbyDoesntExist;
        }

    }
}
