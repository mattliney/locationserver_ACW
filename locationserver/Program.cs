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
        //Normal Variables
        static string mCurrentProtocol = "whois";
        static string mName = null;
        static string mLocation = null;
        static int mTimeout = 1000;

        //Debug Variables
        static bool mDebug = false;
        static string mMessageSent = null;
        static string mMessageReceived = null;

        //Log Variables
        static string mLogFilePath = null;

        //Dictionary Saving Variables
        static Dictionary<string, string> mPeople = new Dictionary<string, string>();
        static string mDatabaseFilePath = null;
        static bool mSaveDatabase = false;

        static void Main(string[] args)
        {
            mPeople.Add("638298", "is in the lab");
            CheckCommandLineArgs(args);
            if (mSaveDatabase) { LoadDictionary(); }
            RunServer();
        }

        static void CheckCommandLineArgs(string[] pArgs)
        {
            for(int i = 0; i < pArgs.Length; i++)
            {
                if(pArgs[i] == "-d")
                {
                    mDebug = true;
                }
                else if (pArgs[i] == "-t")
                {
                    mTimeout = int.Parse(pArgs[++i]);
                }
                else if (pArgs[i] == "-l")
                {
                    mLogFilePath = pArgs[++i];
                }
                else if(pArgs[i] == "-f")
                {
                    mDatabaseFilePath = pArgs[++i];
                    mSaveDatabase = true;
                }
            }
        }

        static void RunServer()
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
                    if (mDebug) { Console.WriteLine("Waiting for Connection..."); }
                    connection = listener.AcceptSocket();
                    if (mDebug) { Console.WriteLine("Connection Received From: " + connection.RemoteEndPoint); }
                    socketStream = new NetworkStream(connection);
                    socketStream.ReadTimeout = mTimeout;
                    socketStream.WriteTimeout = mTimeout;
                    DoRequest(socketStream);
                    socketStream.Close();
                    connection.Close();
                    if (mSaveDatabase) { SaveDictionary(); }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void WhoIsResponse(StreamWriter pWriter)
        {
            if (mLocation == null)
            {
                if (mPeople.ContainsKey(mName))
                {
                    pWriter.WriteLine(mPeople[mName]);
                    mMessageSent = mPeople[mName];
                    pWriter.Flush();
                }
                else
                {
                    pWriter.WriteLine("ERROR: no entries found");
                    mMessageSent = "ERROR: no entries found";
                    pWriter.Flush();
                }
            }
            else
            {
                if (mPeople.ContainsKey(mName))
                {
                    mPeople[mName] = mLocation;
                    pWriter.WriteLine("OK\r\n");
                    mMessageSent = "OK";
                    pWriter.Flush();
                }
                else
                {
                    mPeople.Add(mName, mLocation);
                    pWriter.WriteLine("OK\r\n");
                    mMessageSent = "OK";
                    pWriter.Flush();
                }
            }
        }

        static void SplitArgs(string pArgument)
        {
            if(mCurrentProtocol == "whois")
            {
                string[] arguments = new string[2];
                char[] characters = { ' ', '"' };
                arguments = pArgument.Split(characters, 2);

                mName = arguments[0].Replace("\r\n", string.Empty);

                if (arguments.Length == 2)
                {
                    mName = arguments[0].Replace("\r\n", string.Empty);
                    mLocation = arguments[1].Replace("\r\n", string.Empty);
                }
            }
            else if(mCurrentProtocol == "HTTP/0.9")
            {
                string[] arguments = new string[2];
                char[] characters = { '\n', '\r' };

                arguments = pArgument.Split(characters, StringSplitOptions.RemoveEmptyEntries);
                mName = arguments[0].Remove(0, 5);
                if (arguments.Length == 2)
                {
                    mLocation = arguments[1];
                }
            }
            else if(mCurrentProtocol == "HTTP/1.0")
            {
                string[] temp = new string[1];
                string[] arguments = new string[3];
                char[] characters = { '\n', '\r' };

                arguments = pArgument.Split(characters, StringSplitOptions.RemoveEmptyEntries);
                if (arguments[0].StartsWith("GET /?"))
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
            }
            else if(mCurrentProtocol == "HTTP/1.1")
            {
                string[] temp = new string[1];
                string[] arguments = new string[3];
                char[] characters = { '\n', '\r' };

                arguments = pArgument.Split(characters, StringSplitOptions.RemoveEmptyEntries);
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
            }

            mMessageReceived = mName + " " + mLocation;
        }

        static void HttpResponse(StreamWriter pWriter)
        {
            if (mLocation == null)
            {
                if (mPeople.ContainsKey(mName))
                {
                    pWriter.WriteLine(mCurrentProtocol + " 200 OK\r\nContent-Type: text/plain\r\n\r\n" + mPeople[mName] + "\r\n");
                    mMessageSent = mCurrentProtocol + " 200 OK\r\nContent-Type: text/plain\r\n\r\n" + mPeople[mName] + "\r\n";
                    pWriter.Flush();
                }
                else
                {
                    pWriter.WriteLine(mCurrentProtocol + " 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    mMessageSent = mCurrentProtocol + " 404 Not Found\r\nContent-Type: text/plain\r\n\r\n";
                    pWriter.Flush();
                }
            }
            else
            {
                if (mPeople.ContainsKey(mName))
                {
                    mPeople[mName] = mLocation;
                    pWriter.WriteLine(mCurrentProtocol + "  200 OK\r\nContent-Type: text/plain\r\n\r\n");
                    mMessageSent = mCurrentProtocol + "  200 OK\r\nContent-Type: text/plain\r\n\r\n";
                    pWriter.Flush();
                }
                else
                {
                    mPeople.Add(mName, mLocation);
                    pWriter.WriteLine(mCurrentProtocol + " 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                    mMessageSent = mCurrentProtocol + "  200 OK\r\nContent-Type: text/plain\r\n\r\n";
                    pWriter.Flush();
                }
            }
        }

        static void DebugOutput()
        {
            Console.WriteLine("Message Received: " + mMessageReceived);
            Console.WriteLine("Message Sent: " + mMessageSent);
        }

        static void DoRequest(NetworkStream pListener)
        {
            try
            {
                mName = null;
                mLocation = null;
                mMessageReceived = null;
                mMessageSent = null;
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

                switch(mCurrentProtocol)
                {
                    case "whois":
                        SplitArgs(argument);
                        WhoIsResponse(writer);
                        break;
                    case "HTTP/0.9":
                        SplitArgs(argument);
                        HttpResponse(writer);
                        break;
                    case "HTTP/1.0":
                        SplitArgs(argument);
                        HttpResponse(writer);
                        break;
                    case "HTTP/1.1":
                        SplitArgs(argument);
                        HttpResponse(writer);
                        break;
                }

                if (mDebug) { DebugOutput(); }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    
        static void LoadDictionary()
        {
            if(mDatabaseFilePath != null)
            {
                try
                {
                    StreamReader reader = new StreamReader(mDatabaseFilePath);
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if(line == null || line == "")
                        {
                            break;
                        }
                        char[] c = { ' ' };
                        string[] split = line.Split(c,2);
                        mPeople.Add(split[0], split[1]);
                    }
                    reader.Close();
                }
                catch { }
            }
        }

        static void SaveDictionary()
        {
            if (mDatabaseFilePath != null)
            {
                StreamWriter writer = new StreamWriter(mDatabaseFilePath);

                foreach(KeyValuePair<string,string> kvp in mPeople)
                {
                    writer.WriteLine(kvp.Key + " " + kvp.Value);
                }

                writer.Close();
            }
        }
    }
}
