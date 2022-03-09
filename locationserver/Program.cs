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
        static string mName = null;
        static string mLocation = null;

        static void Main(string[] args)
        {
            mPeople.Add("638298", "is in the lab");
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
                mName = null;
                mLocation = null;

                StreamWriter writer = new StreamWriter(pListener);
                StreamReader reader = new StreamReader(pListener);

                string argument = "";

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

                    mName = arguments[0].Replace("\r\n", string.Empty);

                    if (arguments.Length == 2)
                    {
                        mName = arguments[0].Replace("\r\n", string.Empty);
                        mLocation = arguments[1].Replace("\r\n", string.Empty);
                    }

                    if (mLocation == null)
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
                    else
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
                    if(arguments.Length == 2)
                    {
                        mLocation = arguments[1];
                    }

                    if (mLocation == null)
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                            writer.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + mPeople[mName] + "\r\n");
                            writer.Flush();
                        }
                        else
                        {
                            writer.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            writer.Flush();
                        }
                    }
                    else
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                            mPeople[mName] = mLocation;
                            writer.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            writer.Flush();
                        }
                        else
                        {
                            mPeople.Add(mName, mLocation);
                            writer.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            writer.Flush();
                        }
                    }
                }
                else if (mCurrentProtocol == "HTTP/1.0")
                {
                    string[] temp = new string[1];
                    string[] arguments = new string[3];
                    char[] characters = { '\n', '\r' };

                    arguments = argument.Split(characters, StringSplitOptions.RemoveEmptyEntries);
                    if(arguments[0].StartsWith("GET /?"))
                    {
                        temp = arguments[0].Split(' ');
                        mName = temp[1].Remove(0, 2);
                    }
                    else
                    {
                        temp = arguments[0].Split(' ');
                        mName = temp[1].Remove(0, 1);
                        mLocation = arguments[2];
                    }

                    if (mLocation == null)
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                            writer.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + mPeople[mName]);
                            writer.Flush();
                        }
                        else
                        {
                            writer.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            writer.Flush();
                        }
                    }
                    else
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                            mPeople[mName] = mLocation;
                            writer.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            writer.Flush();
                        }
                        else
                        {
                            mPeople.Add(mName, mLocation);
                            writer.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                            writer.Flush();
                        }
                    }
                }
                else if (mCurrentProtocol == "HTTP/1.1")
                {
                    //split args

                    if (mLocation == null)
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        if (mPeople.ContainsKey(mName))
                        {
                        }
                        else
                        {
                        }
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
