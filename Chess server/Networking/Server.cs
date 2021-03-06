﻿using System;
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
    // ___lobby Codes(10-30)___
    //
    // __client msg(10-15)__
    // 10 - client asks for a lobby to be opened 
    // 11 - client asks to close a lobby
    // 12 - client asks to join a lobby
    // 13 - host asks to kick a player
    // 14 - host asks to start the game
    // 15 - player asks to leave lobby(TODO)
    //
    // __server notifications(16-17)__
    // 16 - a player has joined the lobby
    // 17 - game started
    //
    // __server responses(20 - 30)__
    // 20 - lobby succsessfuly created
    // 21 - lobby succsessfuly joined
    // 22 - lobby succsessfuly closed
    // 23 - something went wrong with opening a lobby
    // 24 - something went wrong with closing a lobby
    // 25 - the lobby the player was trying to join is full
    // 26 - the lobby the player was trying to join does not exist
    // 27 - the player has been kicked from the lobby
    // 28 - the player asks for all the available lobbies
    // 29 - the server sends the player all the available lobbies
    // 30 - Couldn't start game(no second player)
    //
    //
    // ___game Codes(35-45)___
    //
    // __client msg(35)__
    // 35 - make a move
    //
    // __server notifications(40-45)__
    // 40 - send all possible moves for the player
    // 41 - invalid move
    // 42 - white victory
    // 43 - black victory
    // 44 - move piece
    // 45 - draw(ToDo)
    enum msgCodes
    {
        userLogin = 1,
        userSignup,
        loginConfirm,
        signupConfirm,
        infoDoesntMatch, 
        userExists,
        playerConnected,
        CreateLobby = 10,
        CloseLobby,
        JoinLobby,
        KickPlayer,
        StartGame,
        PlayerJoined = 16,
        GameStarted,
        LobbyCreated = 20,
        LobbyJoined,
        LobbyClosed, 
        CouldntOpenLobby,
        CantCloseLobby,
        LobbyFull,
        LobbyDoesntExist,
        Kicked, 
        GetLobbies,
        Lobbies,
        CouldntStart,
        MakeAMove = 35,
        AllMoves = 40,
        InvalidMove,
        BlackWon,
        WhiteWon,
        PieceMove
    }

    class Server
    {
        //global vars
        const int PORT = 5002;
        static TcpListener listener;

        

        /// <summary>
        /// opens the listening socket and starts accepting clients
        /// on an infinite loop.
        /// creates a thread for each client
        /// </summary>
        public static void Listen()
        {
            try
            {
                //create a new listening socket on port PORT
                listener = new TcpListener(IPAddress.Parse("192.168.1.180"), PORT);
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

        /// <summary>
        /// takes care of each client
        /// </summary>
        /// <param name="cl">the client object</param>
        private static void HandleClient(object cl)
        {
            //turn the client object into a TcpClient type(we get it as an object because threads can only get objects)
            if (!(cl is TcpClient))
                throw new Exception("couldn't convert client");
            TcpClient client = (TcpClient)cl;

            NetworkStream stream = client.GetStream();
            string username = string.Empty;
            Lobby lobby = null;

            try
            {
                //player login
                while(username == string.Empty)
                    username = LoginManager.UserLogin(client, stream);
                //after loging in the player can only join lobbies and play games until he quits
                while(true)
                {
                    lobby = LobbyManager.PlayerNavagation(username, stream);
                    ChessGame game = lobby.WaitForStart(username, stream);
                    if (game != null)
                    {
                        game.Play(username, stream);
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



        /// <summary>
        /// receives a message from a network stream
        /// </summary>
        /// <param name="stream">the stream</param>
        /// <returns>the message</returns>
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
