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
        static Dictionary<string, Tuple<string, string>> lobbies = new Dictionary<string, Tuple<string, string>>();

        //while in the menu if a player sends a message this function will navagate the answer to him
        public static string PlayerNavagation(string username, NetworkStream stream)
        {
            while(true)
            {
                //recive msg from player
                string msg = Server.ReceiveMsg(stream);
                LobbyMsg lby = JsonConvert.DeserializeObject<LobbyMsg>(msg);
                switch (lby.Code)
                {
                    case (int)msgCodes.GetLobbies:
                        //get all lobbies that aren't full
                        foreach(KeyValuePair<string, Tuple<string, string>> entry in lobbies)
                        {
                            if(entry.Value.Item2 == null)
                                msg += entry.Key  + entry.Value.ToString() + "\n";
                        }
                        msg = msgCodes.Lobbies.ToString() + "\n" + msg;
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        break;
                    case (int)msgCodes.JoinLobby:
                        //join lobby, if succsusful return lobby name
                        msg = ((int)JoinLobby(username, lby.LobbyName)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        if (msg == ((int)msgCodes.LobbyJoined).ToString())
                            return lby.LobbyName;
                        break;
                    case (int)msgCodes.CreateLobby:
                        //create lobby, if succsusful return lobby name
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
            lobbies[username] = new Tuple<string, string>(username, null);
            lobbyDictMutex.ReleaseMutex();
        }

        public static msgCodes JoinLobby(string username, string lobbyname)
        {
            lobbyDictMutex.WaitOne();
            if (lobbies.ContainsKey(lobbyname))
            {
                if (lobbies[lobbyname].Item2 == null)
                {
                    lobbies[lobbyname] = new Tuple<string, string>(lobbies[lobbyname].Item1, username);
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
