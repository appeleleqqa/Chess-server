using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Chess_server
{
    //this class takes care of all things user related
    class LoginManager
    {
        private static Mutex userListMutex = new Mutex();
        public static Dictionary<string, TcpClient> users = new Dictionary<string, TcpClient>();


        //TODO: ADD MD5 TO PASSWORD
        public static string UserLogin(TcpClient client, NetworkStream stream)
        {
            while (true)
            {
                string data = Server.ReceiveMsg(stream);
                if (data == null)//checks if the client responded
                    throw new Exception();//server will catch this exception at the handle client function causing the server to disconnect the player

                UserLogin user = JsonConvert.DeserializeObject<UserLogin>(data);
                user.Password = CreateHash(user.Password);
                userListMutex.WaitOne();
                if (!users.ContainsKey(user.Username))
                {
                    bool hasUser = false;
                    string msg = null;
                    switch (user.Code)
                    {
                        case (int)msgCodes.userLogin:
                            hasUser = true;
                            break;
                        case (int)msgCodes.userSignup:
                            break;
                        default:
                            return null;//invalid argument
                    }
                    //login - checks if username and password match
                    //signup - checks if the user already exists and creates one if it doesn't
                    if (hasUser ? Database.CheckPassword(user.Username, user.Password) : Database.AddUser(user.Username, user.Password))
                    {
                        users[user.Username] = client;
                        msg = (hasUser ? (int)msgCodes.loginConfirm : (int)msgCodes.signupConfirm).ToString();
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        Console.WriteLine(user.Username +" has joined");
                        return user.Username;
                    }
                    
                    //if the condition doesn't meet the server send one of the following:
                    //login - username and password don't match
                    //signup - user already exits
                    msg = (hasUser ? (int)msgCodes.infoDoesntMatch : (int)msgCodes.userExists).ToString();
                    stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                }
                //user already in the server
                else
                {
                    string msg = ((int)msgCodes.playerConnected).ToString();
                    stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                }
                userListMutex.ReleaseMutex();
            }
        }

        public static void LogOut(string username)
        {
            userListMutex.WaitOne();
            users.Remove(username);
            userListMutex.ReleaseMutex();
        }
        
        public static string CreateHash(string pass)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(pass);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }
}
