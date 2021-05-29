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
	//a class representing a lobby
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

		/// <summary>
		/// creates a new lobby, if lobby creation fails throws an exception
		/// </summary>
		/// <param name="username">host's username</param>
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


		/// <summary>
		/// tries to join a lobby
		/// </summary>
		/// <param name="username">player's name</param>
		/// <param name="hostname">host's name</param>
		/// <returns>both a msg code that says if opening the lobby was successful or not
		///			 and a lobby object(if joining the lobby fail this will be null</returns>
		public static  Tuple<msgCodes, Lobby> JoinLobby(string username, string hostname)
        {
			lobbiesMutex.WaitOne();
			//first checks if the lobby exists
			if(!existingLobbies.ContainsKey(hostname))
				return new Tuple<msgCodes, Lobby>(msgCodes.LobbyDoesntExist, null);
			//then checks if it is joinable
			if (existingLobbies[hostname].IsJoinable())
            {
				//lets the player join the lobby
				existingLobbies[hostname].player2 = username;
				LobbyMsg lby = new LobbyMsg();
				lby.Username = username;
				lby.Code = (int)msgCodes.PlayerJoined;
				string msg = JsonConvert.SerializeObject(lby);
				existingLobbies[hostname].hostStream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
				Tuple < msgCodes, Lobby > retVal = new Tuple<msgCodes, Lobby>(msgCodes.LobbyJoined, existingLobbies[hostname]);
				lobbiesMutex.ReleaseMutex();
				return retVal;
			}
			lobbiesMutex.ReleaseMutex();
			return new Tuple<msgCodes, Lobby>(msgCodes.LobbyFull, null);
        }

		/// <summary>
		/// checks if a lobby is joinable
		/// </summary>
		/// <returns>whether or not the lobby is joinable</returns>
		public bool IsJoinable()
        {
			if (player2 == string.Empty)
				return true;
			return false;
		}
		
		/// <summary>
		/// finds all the lobbies that are joinable
		/// </summary>
		/// <returns>all joinable lobbies</returns>
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
		/// <summary>
		/// closes a lobby by hostname
		/// </summary>
		/// <param name="hostname">hostname</param>
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
				lobbiesMutex.WaitOne();
				existingLobbies.Remove(hostname);
				lobbiesMutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// sends each player to the function he should be waiting in for the game to start
		/// </summary>
		/// <param name="username">the player's name</param>
		/// <param name="stream">the player's network stream</param>
		/// <returns></returns>
		public ChessGame WaitForStart(string username, NetworkStream stream)
        {
			if (hostName == username)
				return HostLobby(stream);
			return PlayerLobby(stream);
        }

		/// <summary>
		/// the function the host "waits" on for the game to start,
		/// gives him all the abilities to kick or close the lobby or start the game
		/// </summary>
		/// <param name="stream">the player's stream</param>
		/// <returns>the game object(if the game didn't start returns null)</returns>
		private ChessGame HostLobby(NetworkStream stream)
        {
			string msg;
			hostStream = stream;
			//receive commands from the host
			while (true)
			{
				hostMsg = (msgCodes)int.Parse(Server.ReceiveMsg(stream));
				if (hostMsg == msgCodes.KickPlayer)
					msgAvb = true;
				//if the commands makes the lobby close get our of the receive loop
				else if (hostMsg == msgCodes.CloseLobby || hostMsg == msgCodes.StartGame)
					break;
			}
			switch (hostMsg)
            {
				case msgCodes.CloseLobby:
					CloseLobby(hostName);
					return null;
				case msgCodes.StartGame:
					//check if there is another player
					if (player2 != string.Empty)
					{
						//create a game and notify both the host and second player
						game = new ChessGame(hostName);
						msg = ((int)msgCodes.GameStarted).ToString();
						stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
						msgAvb = true;
						return game;
					}
					else
                    {
						msg = ((int)msgCodes.CouldntStart).ToString();
						stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
					}
					break;
            }				
			return null;
        }


		/// <summary>
		/// the function the player "waits" on while waiting for a game to start
		/// just waits for the host to send something and acts on it
		/// </summary>
		/// <param name="stream">the player's name</param>
		/// <returns>the game object(if the game didn't start returns null)</returns>
		private ChessGame PlayerLobby(NetworkStream stream)
        {
			while (!msgAvb) ;//wait for the host to send a message
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
					msg = ((int)msgCodes.GameStarted).ToString();
					stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
					existingLobbies.Remove(hostName);
					return game;
            }
			return null;
        }
	}
}
