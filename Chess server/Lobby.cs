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
		private NetworkStream hostStream = null;
		private string player2 = string.Empty;
		private NetworkStream player2Stream;
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
				LobbyMsg lby = new LobbyMsg();
				lby.Username = username;
				lby.Code = (int)msgCodes.PlayerJoined;
				string msg = JsonConvert.SerializeObject(lby);
				existingLobbies[hostname].hostStream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
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
		public static List<string> JoinableLobbiesJson()
		{
			List<string> lobbies = new List<string>();
			lobbiesMutex.WaitOne();
			foreach (KeyValuePair<string, Lobby> entry in Lobby.existingLobbies)
			{
				if (entry.Value.IsJoinable())
					lobbies.Add(entry.Key);
			}
			lobbiesMutex.ReleaseMutex();
			return lobbies;
		}

		//closes a lobby
		public static void CloseLobby(string hostname)
		{
			Lobby lby = existingLobbies[hostname];

			if (lby.player2 != string.Empty)
            {
				lby.hostMsg = msgCodes.LobbyClosed;
				lby.WaitForAction.ReleaseMutex();
			}
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
			hostStream = stream;
			while (true)
			{
				hostMsg = (msgCodes)int.Parse(Server.ReceiveMsg(stream));
				if (hostMsg != msgCodes.KickPlayer)
					break;
				WaitForAction.ReleaseMutex();
			}
			switch (hostMsg)
            {
				case msgCodes.CloseLobby:
					CloseLobby(hostName);
					return null;
				case msgCodes.StartGame:
					break;
            }				
			//create game
			//return game
			return null;
        }

		private Game PlayerLobby(NetworkStream stream)
        {
			WaitForAction.WaitOne();
			string msg;
			switch (hostMsg)
            {
				case msgCodes.KickPlayer:
					msg = ((int)msgCodes.Kicked).ToString();
					stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
					player2 = string.Empty;
					return null;
				case msgCodes.LobbyClosed:
					msg = ((int)msgCodes.LobbyClosed).ToString();
					stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
					return null;
				case msgCodes.StartGame:
					//return game object
					break;
            }
			return null;
        }
	}
}
