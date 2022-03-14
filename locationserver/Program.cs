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
                    Console.WriteLine("Waiting for Connection...");
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

        static void WhoIs(StreamWriter pWriter)
        {
            if (mLocation == null)
            {
                if (mPeople.ContainsKey(mName))
                {
                    pWriter.WriteLine(mPeople[mName]);
                    pWriter.Flush();
                }
                else
                {
                    pWriter.WriteLine("ERROR: no entries found");
                    pWriter.Flush();
                }
            }
            else
            {
                if (mPeople.ContainsKey(mName))
                {
                    mPeople[mName] = mLocation;
                    pWriter.WriteLine("OK\r\n");
                    pWriter.Flush();
                }
                else
                {
                    mPeople.Add(mName, mLocation);
                    pWriter.WriteLine("OK\r\n");
                    pWriter.Flush();
                }
            }
        }

        static void HTTP(StreamWriter pWriter)
        {
            if (mLocation == null)
            {
                if (mPeople.ContainsKey(mName))
                {
                    pWriter.WriteLine(mCurrentProtocol + " 200 OK\r\nContent-Type: text/plain\r\n\r\n" + mPeople[mName] + "\r\n");
                    pWriter.Flush();
                }
                else
                {
                    pWriter.WriteLine(mCurrentProtocol + " 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    pWriter.Flush();
                }
            }
            else
            {
                if (mPeople.ContainsKey(mName))
                {
                    mPeople[mName] = mLocation;
                    pWriter.WriteLine(mCurrentProtocol + "  200 OK\r\nContent-Type: text/plain\r\n\r\n");
                    pWriter.Flush();
                }
                else
                {
                    mPeople.Add(mName, mLocation);
                    pWriter.WriteLine(mCurrentProtocol + " 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                    pWriter.Flush();
                }
            }
        }

        static void doRequest(NetworkStream pListener)
        {
            Console.WriteLine("Connection Established");
            try
            {
                mName = null;
                mLocation = null;
                mCurrentProtocol = "whois";
                int lineCount = 0;

                StreamWriter writer = new StreamWriter(pListener);
                StreamReader reader = new StreamReader(pListener);

                string argument = "";

                try
                {
                    while(reader.Peek() != -1)
                    {
                        argument += (char)reader.Read();
                    }
                }
                catch { }


                char[] chars = { '\r', '\n' };
                string[] split = argument.Split(chars, StringSplitOptions.RemoveEmptyEntries);
                lineCount = split.Length;


                if (split[0].EndsWith("HTTP/1.0"))
                {
                    mCurrentProtocol = "HTTP/1.0";
                }
                else if (split[0].EndsWith("HTTP/1.1"))
                {
                    mCurrentProtocol = "HTTP/1.1";
                }
                else if (split[0].StartsWith("GET /") && lineCount == 1|| split[0].StartsWith("PUT /") && lineCount == 2)
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

                    WhoIs(writer);
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

                    HTTP(writer);
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

                    HTTP(writer);
                }
                else if (mCurrentProtocol == "HTTP/1.1")
                {
                    string[] temp = new string[1];
                    string[] arguments = new string[3];
                    char[] characters = { '\n', '\r' };

                    arguments = argument.Split(characters, StringSplitOptions.RemoveEmptyEntries);
                    if (arguments[0].StartsWith("GET /?"))
                    {
                        temp = arguments[0].Split(' ');
                        mName = temp[1].Remove(0, 7);
                    }
                    else
                    {
                        temp = arguments[3].Split('&');
                        mName = temp[0].Remove(0, 5);
                        mLocation = temp[1].Remove(0, 9);
                    }

                    HTTP(writer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
