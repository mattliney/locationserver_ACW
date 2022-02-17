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
            mPeople.Add("Billy", "Somewhere");
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
            Console.WriteLine("Connection Established");
            try
            {
                StreamWriter writer = new StreamWriter(pListener);
                StreamReader reader = new StreamReader(pListener);

                string argument = reader.ReadLine();
                string[] arguments = new string[2];
                char[] characters = {' ', '"' };
                arguments = argument.Split(characters, 2);

                if(arguments.Length == 1)
                {
                    arguments[0] = arguments[0].Replace("\r\n", string.Empty);
                }
                else if(arguments.Length == 2)
                {
                    arguments[0] = arguments[0].Replace("\r\n", string.Empty);
                    arguments[1] = arguments[1].Replace("\r\n", string.Empty);
                }

                // Take in the arguments, split them. Arguments 0 is the name, Arguments 1 is the location

                if(arguments.Length == 1) //Request user location
                {
                    if(mPeople.ContainsKey(arguments[0]))
                    {
                        Console.WriteLine(arguments[0] + " is in " + mPeople[arguments[0]]);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: This user does not exist");
                    }
                }
                else if(arguments.Length == 2) //Update location or add user
                {
                    if (mPeople.ContainsKey(arguments[0]))
                    {
                        mPeople[arguments[0]] = arguments[1];
                        Console.WriteLine(arguments[0] + " is in " + mPeople[arguments[0]]);
                    }
                    else
                    {

                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
