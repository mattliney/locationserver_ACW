﻿using System;
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

                string argument = reader.ReadLine();

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
                        arguments[0] = arguments[0].Replace("\r\n", string.Empty);
                    }
                    else if (arguments.Length == 2)
                    {
                        arguments[0] = arguments[0].Replace("\r\n", string.Empty);
                        arguments[1] = arguments[1].Replace("\r\n", string.Empty);
                    }

                    if (arguments.Length == 1)
                    {
                        if (mPeople.ContainsKey(arguments[0]))
                        {
                            writer.WriteLine(mPeople[arguments[0]]);
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
                        if (mPeople.ContainsKey(arguments[0]))
                        {
                            mPeople[arguments[0]] = arguments[1];
                            writer.WriteLine("OK\r\n");
                            writer.Flush();
                        }
                        else
                        {
                            mPeople.Add(arguments[0], arguments[1]);
                            writer.WriteLine("OK\r\n");
                            writer.Flush();
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
