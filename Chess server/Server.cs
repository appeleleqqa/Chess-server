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
    //the codes the server and client use to know what to do or tell eachother what to do
    // ___login codes(1-7)____
    //
    // __client msg(1-2)__
    // 1 - client requests login
    // 2 - client requests signup
    //
    // __server resposes(3-7)__
    // 3 - server confirms login
    // 4 - server confirms signup
    // 5 - password and username of the user do not match
    // 6 - the user you are trying to create already exists
    // 7 - the user you are trying to log into is already in the server
    //
    //
    // ___lobby Codes(10-14)___
    //
    // __client msg(10-12)__
    // 10 - client asks for a lobby to be opened 
    // 11 - client asks to close a lobby
    // 12 - client asks to join a lobby
    //
    // __server responses(13-15)
    // 13 - lobby succsessfuly created
    // 14 - lobby succsessfuly joined
    // 15 - lobby succsessfuly closed
    // 16 - something went wrong with opening a lobby
    // 17 - something went wrong with closing a lobby
    // 18 - the lobby the player was trying to join is full
    // 19 - the lobby the player was trying to join does not exist
    // 20 - the player has been kicked from the lobby
    // 21 - the player asks for all the available lobbies
    // 22 - the server sends the player all the available lobbies
    enum msgCodes
    {
        userLogin = 1,
        userSignup,
        loginConfirm,
        signupConfirm,
        infoDoesntMatch, // 5
        userExists,
        playerConnected,
        CreateLobby = 10,
        CloseLobby,
        JoinLobby,
        LobbyCreated,
        LobbyJoined,
        LobbyClosed, // 15
        CouldntOpenLobby,
        CantCloseLobby,
        LobbyFull,
        LobbyDoesntExist,
        Kicked, //20
        GetLobbies,
        Lobbies,
        StartGame = 30,
        GameStarted,
    }

    class Server
    {
        //global vars
        const int PORT = 5002;
        static TcpListener listener;

        //opens a listening socket that accept clients on an infinite loop
        //creates a thread for each client
        public static void Listen()
        {
            try
            {
                //create a new listening socket on port PORT
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), PORT);
                listener.Start();
                Console.WriteLine("Waiting for connections...");

                //start to accept clients
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("client accepted");
                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }

        //a function that is used to handel clients one by one(this will be run as a thread)
        private static void HandleClient(object cl)
        {
            if (!(cl is TcpClient))
                throw new Exception("couldn't convert client");
            TcpClient client = (TcpClient)cl;

            NetworkStream stream = client.GetStream();
            string username = string.Empty;
            Lobby lobby = null;

            try
            {
                while(username == string.Empty)
                    username = LoginManager.UserLogin(client, stream);
                //this loop will work till a player has left, causing an Exception to be thrown
                while(true)
                {
                    lobby = LobbyManager.PlayerNavagation(username, stream);
                    //if(lobby.WaitForStart == null)
                    {

                    }
                }


            }
            catch
            { /*Console.WriteLine("Exception: {0}", e.Message);*/ }
            finally
            {
                // User left
                Console.WriteLine(username != null ? username + " has left" : "Client left");
                stream.Close();
                if (username != null)
                    LoginManager.LogOut(username);
                client.Close();
            }

        }




        // Recives a 256 bytes long msg 
        public static string ReceiveMsg(NetworkStream stream)
        {
            byte[] myReadBuffer = new byte[1024];
            StringBuilder myCompleteMessage = new StringBuilder();
            int numberOfBytesRead = 0;
            try
            {
                // Incoming message may be larger than the buffer size.
                do
                {
                    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                }
                while (stream.DataAvailable);

            }
            catch
            {
                return null;
            }

            return myCompleteMessage.ToString();
        }
    }
}
