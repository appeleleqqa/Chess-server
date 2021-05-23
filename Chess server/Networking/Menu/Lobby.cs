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
		private bool msgAvb = false;
		private msgCodes hostMsg;
		private ChessGame game = null;

		//creates a new lobby
		//throws an exception on fail
		public Lobby(string username)
		{
			//check if the lobby already exists
			lobbiesMutex.WaitOne();
			if(existingLobbies.ContainsKey(username))
			{
				lobbiesMutex.ReleaseMutex();
				throw new Exception();
			}
			existingLobbies.Add(username, this);
			lobbiesMutex.ReleaseMutex();

			hostName = username;
			msgAvb = false;
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
			if (existingLobbies.ContainsKey(hostname))
			{
				Lobby lby = existingLobbies[hostname];
				if (lby.player2 != string.Empty)
				{
					lby.hostMsg = msgCodes.LobbyClosed;
					lby.msgAvb = true;
				}
				existingLobbies.Remove(hostname);
			}
		}

		//this function is called right after a player joins the lobby
		//it calls a function that is diffrent for the host and the 2nd player
		public ChessGame WaitForStart(string username, NetworkStream stream)
        {
			if (hostName == username)
				return HostLobby(stream);
			return PlayerLobby(stream);
        }

		//the function that runs while a player is in a lobby he created
		//lets the host send commands to the server(kick, close lobby and start game)
		private ChessGame HostLobby(NetworkStream stream)
        {
			hostStream = stream;
			while (true)
			{
				hostMsg = (msgCodes)int.Parse(Server.ReceiveMsg(stream));
				if (hostMsg == msgCodes.KickPlayer)
					msgAvb = true;
				else if (hostMsg == msgCodes.CloseLobby || hostMsg == msgCodes.StartGame)
					break;
			}
			switch (hostMsg)
            {
				case msgCodes.CloseLobby:
					CloseLobby(hostName);
					return null;
				case msgCodes.StartGame:
					game = new ChessGame(hostName);
					msgAvb = true;
					return game;
            }				
			return null;
        }

		//the function that runs while a player is in a lobby he joined
		//waits for the host to send a message and sents it to the player
		private ChessGame PlayerLobby(NetworkStream stream)
        {
			while (!msgAvb) ;
			msgAvb = false;
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
					return game;
            }
			return null;
        }
	}
}
