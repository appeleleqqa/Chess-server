using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
                    Console.WriteLine("Client accepted.");
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

            throw new NotImplementedException();
        }
    }
}
