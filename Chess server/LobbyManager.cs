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
        //while in the menu if a player sends a message this function will navagate the answer to him
        public static Lobby PlayerNavagation(string username, NetworkStream stream)
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
                        msg = Lobby.JoinableLobbiesJson();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        break;
                    case (int)msgCodes.JoinLobby:
                        //join lobby, if succsusful return lobby name
                        Tuple<msgCodes, Lobby> lobbyT = Lobby.JoinLobby(username, lby.HostName);
                        msg = ((int)lobbyT.Item1).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        if (msg == ((int)msgCodes.LobbyJoined).ToString())
                            return lobbyT.Item2;
                        break;
                    case (int)msgCodes.CreateLobby:
                        //create lobby, if succsusful return lobby name
                        Lobby lobby = null;
                        try
                        {
                            lobby = new Lobby(username);
                            msg = ((int)msgCodes.LobbyCreated).ToString();
                        }
                        catch
                        {
                            msg = ((int)msgCodes.CouldntOpenLobby).ToString();
                        }
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        if (msg == ((int)msgCodes.LobbyJoined).ToString())
                            return lobby;
                        break;
                }
            }
        }


        //after a player joins a lobby, as the host he can decide to kick or start
        //this function will return true when the game starts
        // false can mean 2 things
        //as a host false = you closed the lobby
        //as a player false = you have been kicked
        //public static bool InsideLobby(string hostName, string username, NetworkStream stream)
        //{
        //    if(hostName == username)
        //    {

        //    }

        //}


        //opens a lobby
        //true on success
        //false on failure
        //public static msgCodes OpenLobby(string username)
        //{
        //    try
        //    {
        //        lobbyDictMutex.WaitOne();
        //        if (lobbies.ContainsKey(username))
        //            return msgCodes.CouldntOpenLobby;
        //        lobbies[username] = string.Empty;
        //        lobbyDictMutex.ReleaseMutex();
        //    }
        //    catch
        //    {
        //        return msgCodes.CouldntOpenLobby;
        //    }
        //    return msgCodes.LobbyCreated;
        //}

        //closes lobby(if one exists)
        public static void CloseLobby(string username)
        {
            //if lobby doesn't exist, simply does nothing
            lobbies.Remove(username);
        }

        //checks if the lobby the player tried to join is joinable and returns a msg code depending on the result
        //public static msgCodes JoinLobby(string username, string hostName)
        //{
        //    lobbyDictMutex.WaitOne();
        //    if (lobbies.ContainsKey(username))
        //    {
        //        if (lobbies[hostName] == string.Empty)
        //        {
        //            lobbies[hostName] = username; 
        //            lobbyDictMutex.ReleaseMutex();
        //            return msgCodes.LobbyJoined;
        //        }
        //        return msgCodes.LobbyFull;
        //    }
        //    lobbyDictMutex.ReleaseMutex();
        //    return msgCodes.LobbyDoesntExist;
        //}

    }
}
