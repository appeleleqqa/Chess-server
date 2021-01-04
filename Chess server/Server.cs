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
    public class UserLogin
    {
        public int Code { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class loginMsg
    {
        public int Code { get; set; }
    }

    enum msgCodes
    {
        userLogin = 1,
        userSignup,
        loginConfirm,
        signupConfirm,
        infoDoesntMatch, //5
        userExists,
    }

    class Server
    {
        const int PORT = 5002;
        static TcpListener listener;
        static Dictionary<string, TcpClient> users = new Dictionary<string, TcpClient>();
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

        private static void HandleClient(object cl)
        {
            TcpClient client = (TcpClient)cl;
            if (client == null)
                throw new Exception("cloudn't convert client");
            NetworkStream stream = client.GetStream();
            string username = null;
            try
            {
                username = userLogin(client, stream);

            }
            finally
            {
                // User left
                stream.Close();
                if(username != null)
                    users.Remove(username);
                client.Close();
            }

        }


        public static string userLogin(TcpClient client, NetworkStream stream)
        {
            while (true)
            {
                string data = reciveMsg(stream);
                UserLogin user = JsonConvert.DeserializeObject<UserLogin>(data);

                if (!users.ContainsKey(user.Username))
                {
                    switch (user.Code)
                    {
                        case (int)msgCodes.userLogin:
                            if (Database.CheckPassword(user.Username, user.Password))
                            {
                                users[user.Username] = client;
                                return user.Username;
                            }
                            //send couldn't log in
                            break;
                        case (int)msgCodes.userSignup:
                            if (Database.AddUser(user.Username, user.Password))
                            {
                                users[user.Username] = client;
                                return user.Username;
                            }
                            //send couldn't sign up
                            break;
                    }
                }
                else
                {
                    //user already in server
                }
            }
        }


        // Recives a 256 bytes long msg 
        public static string reciveMsg(NetworkStream stream)
        {
            string data = null;
            Byte[] bytes = new Byte[256];
            int i;
            try
            {
                i = stream.Read(bytes, 0, bytes.Length);
                string hex = BitConverter.ToString(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
                return null;
            }

            return data;
        }
    }
}
