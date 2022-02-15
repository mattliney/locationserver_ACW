using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace location_server
{
    class Program
    {
        static Dictionary<string, string> mPeople = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            runServer();
        }

        static void runServer()
        {
            TcpListener listener;
            Socket connection;
            NetworkStream socketStream;
            try
            {
                listener = new TcpListener(43);
                listener.Start();
                while (true)
                {
                    connection = listener.AcceptSocket();
                    socketStream = new NetworkStream(connection);
                    doRequest(socketStream);
                    socketStream.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void doRequest(NetworkStream pListener)
        {
            StreamWriter writer = new StreamWriter(pListener);
            StreamReader reader = new StreamReader(pListener);

            //string[] input = new string[2];
            //input = reader.ReadToEnd().Split(' ');

            Console.WriteLine("it worked");
        }
    }
}
