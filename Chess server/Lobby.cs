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
	class Lobby
	{
		private static Dictionary<string, Lobby> existingLobbies = new Dictionary<string, Lobby>();
		private static Mutex lobbiesMutex = new Mutex();
		private string hostName;
		private string player2 = string.Empty;
		private Mutex WaitForAction;
		private msgCodes hostMsg;

		//creates a new lobby
		//throws an exception on fail
		public Lobby(string username)
		{
			lobbiesMutex.WaitOne();
			if(existingLobbies.ContainsKey(username))
			{
				lobbiesMutex.ReleaseMutex();
				throw new Exception();
			}
			hostName = username;
			existingLobbies.Add(username, this);
			WaitForAction = new Mutex();
			WaitForAction.WaitOne();
			lobbiesMutex.ReleaseMutex();
		}

		//tries to join a lobby, returns the msgCode that tells you what is the outcome and the lobby you joined
		//if the player didn't manage to join lobby = null
		public static  Tuple<msgCodes, Lobby> JoinLobby(string username, string hostname)
        {
			lobbiesMutex.WaitOne();
			if(!existingLobbies.ContainsKey(hostname))
				return new Tuple<msgCodes, Lobby>(msgCodes.LobbyDoesntExist, null);
			if (existingLobbies[hostname].IsJoinable())
            {
				existingLobbies[hostname].player2 = username;
				return new Tuple<msgCodes, Lobby>(msgCodes.LobbyJoined, existingLobbies[hostname]) ;
            }
			return new Tuple<msgCodes, Lobby>(msgCodes.LobbyFull, null);
        }

		//checks if a lobby is joinable
		public bool IsJoinable()
        {
			if (player2 == string.Empty)
				return true;
			return false;
		}

		//goes over every lobby in the static array of lobbies and check which ones are joinable
		//retruns a json array with the coresponding code and the array of joinable lobbies
		public static string JoinableLobbiesJson()
		{
			string msg = "[";
			lobbiesMutex.WaitOne();
			foreach (KeyValuePair<string, Lobby> entry in Lobby.existingLobbies)
			{
				if (entry.Value.IsJoinable())
					msg += '"' + entry.Key + "\",";
			}
			lobbiesMutex.ReleaseMutex();
			msg = msg.Substring(0, msg.Length - 1)/*get rid of the final comma*/  + "]";
			msg = "{\"Code\":" + msgCodes.Lobbies.ToString() + ", \"Lobbies\":" + msg + "}";
			return msg;
		}

		//closes a lobby
		public static void CloseLobby(string hostname)
		{
			existingLobbies.Remove(hostname);
		}

		public Game WaitForStart(string username, NetworkStream stream)
        {
			if (hostName == username)
				return HostLobby(stream);
			return PlayerLobby(stream);
        }

		private Game HostLobby(NetworkStream stream)
        {
			//while(Server.ReceiveMsg(stream) != msgCodes);

			return null;
        }

		private Game PlayerLobby(NetworkStream stream)
        {
			WaitForAction.WaitOne();
			switch(hostMsg)
            {
				case msgCodes.Kicked:
					//send the player that he got kicked
					player2 = string.Empty;
					return null;
				//game started case
            }
			return null;
        }
	}
}
