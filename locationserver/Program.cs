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
        static string mCurrentProtocol = "whois";
        static string mName;
        static string mLocation;

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
                    socketStream.ReadTimeout = 1000;
                    socketStream.WriteTimeout = 1000;
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

                string argument = "";

                while (argument != "")
                {
                    argument = reader.ReadLine();
                }

                try
                {
                    while (true)
                    {
                        argument += reader.ReadLine() + "\r\n";
                    }
                }
                catch { }

                if (argument.Contains("HTTP/1.0"))
                {
                    mCurrentProtocol = "HTTP/1.0";
                }
                else if (argument.Contains("HTTP/1.1"))
                {
                    mCurrentProtocol = "HTTP/1.1";
                }
                else if (argument.Contains("GET") || argument.Contains("PUT"))
                {
                    mCurrentProtocol = "HTTP/0.9";
                }
                else
                {
                    mCurrentProtocol = "whois";
                }

                if(mCurrentProtocol == "whois")
                {
                    string[] arguments = new string[2];
                    char[] characters = { ' ', '"' };
                    arguments = argument.Split(characters, 2);

                    if (arguments.Length == 1)
                    {
                        mName = arguments[0].Replace("\r\n", string.Empty);
                    }
                    else if (arguments.Length == 2)
                    {
                        mName = arguments[0].Replace("\r\n", string.Empty);
                        mLocation = arguments[1].Replace("\r\n", string.Empty);
                    }

                    if (arguments.Length == 1)
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                            writer.WriteLine(mPeople[mName]);
                            writer.Flush();
                        }
                        else
                        {
                            writer.WriteLine("ERROR: no entries found");
                            writer.Flush();
                        }
                    }
                    else if (arguments.Length == 2) 
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                            mPeople[mName] = mLocation;
                            writer.WriteLine("OK\r\n");
                            writer.Flush();
                        }
                        else
                        {
                            mPeople.Add(mName, mLocation);
                            writer.WriteLine("OK\r\n");
                            writer.Flush();
                        }
                    }
                }
                else if(mCurrentProtocol == "HTTP/0.9")
                {
                    string[] arguments = new string[2];
                    char[] characters = { '\n', '\r' };
                    arguments = argument.Split(characters, StringSplitOptions.RemoveEmptyEntries);
                    mName = arguments[0].Remove(0, 5);
                    mLocation = arguments[1]; //iif lebgth is  no 2
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
