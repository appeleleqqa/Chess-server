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
		private static Dictionary<string, Lobby> existingLobbies;
		private static Mutex lobbiesMutex;
		private string hostName;
		private string player2 = string.Empty;

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
			lobbiesMutex.ReleaseMutex();
		}

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

		public bool IsJoinable()
        {
			if (player2 == string.Empty)
				return true;
			return false;
		}

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

		public static void CloseLobby(string hostname)
		{
			existingLobbies.Remove(hostname);
		}
	}
}
