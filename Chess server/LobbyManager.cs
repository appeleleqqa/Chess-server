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
        // dict[host name] = player 2 name(in the case of an empty spot this will be String.empty)
        static Dictionary<string, string> lobbies = new Dictionary<string, string>();

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
                        msg = "[";
                        foreach(KeyValuePair<string, string> entry in lobbies)
                        {
                            if(entry.Value == string.Empty)
                                msg += '"' + entry.Key + "\",";
                        }
                        msg = msg.Substring(0,msg.Length - 1)/*get rid of the final comma*/  + "]";
                        msg = "{\"Code\":"+msgCodes.Lobbies.ToString()+ ", \"Lobbies\":" + msg + "}";
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        break;
                    case (int)msgCodes.JoinLobby:
                        //join lobby, if succsusful return lobby name
                        msg = ((int)JoinLobby(username, lby.HostName)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        if (msg == ((int)msgCodes.LobbyJoined).ToString())
                            return lby.HostName;
                        break;
                    case (int)msgCodes.CreateLobby:
                        //create lobby, if succsusful return lobby name
                        msg = ((int)OpenLobby(username)).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        if (msg == ((int)msgCodes.LobbyJoined).ToString())
                            return lby.HostName;
                        break;
                }
            }
        }

        //opens a lobby
        //true on success
        //false on failure
        public static msgCodes OpenLobby(string username)
        {
            try
            {
                lobbyDictMutex.WaitOne();
                if (lobbies.ContainsKey(username))
                    return msgCodes.CouldntOpenLobby;
                lobbies[username] = string.Empty;
                lobbyDictMutex.ReleaseMutex();
            }
            catch
            {
                return msgCodes.CouldntOpenLobby;
            }
            return msgCodes.LobbyCreated;
        }

        //closes lobby(if one exists)
        public static void CloseLobby(string username)
        {
            //if lobby doesn't exist, simply does nothing
            lobbies.Remove(username);
        }

        //checks if the lobby the player tried to join is joinable and returns a msg code depending on the result
        public static msgCodes JoinLobby(string username, string hostName)
        {
            lobbyDictMutex.WaitOne();
            if (lobbies.ContainsKey(username))
            {
                if (lobbies[hostName] == string.Empty)
                {
                    lobbies[hostName] = username; 
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
