using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

public class UserLogin
{
    public int Code { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}



namespace Chess_server
{
    class Server
    {
        const int PORT = 5002;
        static TcpListener listener;
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

            string data = reciveMsg(client);
            
            UserLogin user = JsonConvert.DeserializeObject<UserLogin>(data);

            switch (user.Code)
            {

            }

            //writer.Close();
            client.Close();
        }

        public static string reciveMsg(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            string data = null;
            Byte[] bytes = new Byte[256];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("{1}: Received: {0}", data, Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
                client.Close();
            }
            stream.Close();

            return data;
        }
    }
}
