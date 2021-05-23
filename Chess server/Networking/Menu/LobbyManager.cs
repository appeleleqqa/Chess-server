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
                        LobbyListMsg lbyList = new LobbyListMsg();
                        lbyList.Lobbies = Lobby.JoinableLobbiesJson();
                        lbyList.Code = (int)msgCodes.Lobbies;
                        msg = JsonConvert.SerializeObject(lbyList);
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        break;
                    case (int)msgCodes.JoinLobby:
                        //join lobby, if succsusful return lobby name
                        Tuple<msgCodes, Lobby> lobbyT = Lobby.JoinLobby(username, lby.Username);
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
                        if (msg == ((int)msgCodes.LobbyCreated).ToString())
                            return lobby;
                        break;
                }
            }
        }

    }
}
