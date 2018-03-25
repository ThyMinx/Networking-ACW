using System;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace LocationServer
{
    //Class to run threads whenever connected to.
    class Threading
    {
        public static bool debugMode = false;

        public static MTLogging log;

        public static Database database;

        [STAThread]
        static void Main(string[] args)
        {
            TcpListener listener;
            Socket connection;
            Server server;

            int timeout = 1000;
            string fileName = null;
            string databaseName = null;

            if (args.Length == 1)
            {
                if (args[0] == "-d")
                {
                    debugMode = true;
                    Console.WriteLine("Debug mode enabled.");
                }
                else if (args[0] == "-w")
                {
                    Application ui = new Application();
                    ui.Run(new Interface(debugMode, log, database));
                }
                else
                {
                    Console.WriteLine("Arguments invalid.");
                }
            } //~~~~~DEBUGGING~~~~~
            else if (args.Length > 1)
            {
                for (int i = 0; i < args.Length; i++)
                {

                    if (args[i] == "-d")
                    {
                        debugMode = true;
                        Console.WriteLine("Debug Mode Activated.");
                    }
                    else if (args[i] == "-t")
                    {
                        try
                        {
                            timeout = Int32.Parse(args[i + 1]);
                            if (debugMode && timeout != 0) Console.WriteLine("Timeout time set to: " + timeout.ToString() + "ms");
                            else if (debugMode && timeout == 0) Console.WriteLine("Timeout time deactivated.");
                        }
                        catch
                        {
                            if (debugMode) Console.WriteLine("Invalid arguments given. Timeout set to default (1000ms).");
                        }
                    }
                    else if (args[i] == "-l")
                    {
                        try
                        {
                            fileName = args[i + 1];
                            if (debugMode) Console.WriteLine("Log filename set to: " + fileName);
                        }
                        catch
                        {
                            if (debugMode) Console.WriteLine("Invalid file name.");
                        }
                    }
                    else if (args[i] == "-f")
                    {
                        try
                        {
                            databaseName = args[i + 1];
                            if (debugMode) Console.WriteLine("Database filename set to: " + databaseName);
                        }
                        catch
                        {
                            if (debugMode) Console.WriteLine("Invalid file name.");
                        }
                    }
                    else if (args[i] == "-w")
                    {
                        Application ui = new Application();
                        ui.Run(new Interface(debugMode, log, database));
                    }
                }
            } //~~~~~DEBUGGING~~~~~
            else if (args.Length == 0)
            {
                if (debugMode) Console.WriteLine("Everything set to default.");
            }

            if (debugMode) Console.WriteLine("\nServer running...\n~~~~~~~~~~~~~~~~~~~~~~~~~");//~~~~~DEBUGGING~~~~~

            log = new MTLogging(fileName);
            database = new Database(databaseName);
            if (databaseName != null) database.LoadDatabase();

            try
            {
                listener = new TcpListener(IPAddress.Any, 43); //Listens for data packets on port 43.
                listener.Start(); //Starts the listener.

                while (true) //While it is connected.
                {
                    connection = listener.AcceptSocket(); //TCPListner connects a socket
                    if (timeout != 0)
                    {
                        connection.ReceiveTimeout = timeout; //Set timeout
                        connection.SendTimeout = timeout; //Set timeout
                    }

                    server = new Server(); //Creates a new object of server
                    Thread t = new Thread(() => server.RunServer(connection, debugMode, log, database)); //Creates a new thread running the server object with it's method
                    t.Start(); //Starts the thread
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
            }
        }
    }

    //Class to run the server requests.
    class Server
    {
        static Dictionary<string, string> locations; //Database of users and their locations

        static bool debugMode = false; //~~~~~DEBUGGING~~~~~
        static string status = "OK"; //Logging
        static string hostName = null; //Logging
        static string line = null; //Logging
        static MTLogging log = null; //Logging
        static Database database = null; //Database

        enum ProtocolType //An enum to help create the protocol types
        {
            HTTP1,
            HTTP0,
            HTTP9,
            WHOIS
        }

        public void RunServer(Socket connection, bool p_debugMode, MTLogging p_log, Database p_database) //Basic "main" method for the class
        {
            debugMode = p_debugMode; //Setting the debug mode to the one passed in
            log = p_log; //Setting the logging object to the one passed in
            database = p_database; //Database object set to the database object from the main
            locations = database.ReadDatabase(); //Database set to the database in the database object
            hostName = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString(); //Sets the hostname to the IPAddress

            NetworkStream socketStream = new NetworkStream(connection); //Setting the network stream on the socket

            if (debugMode) Console.WriteLine("Connection Established.\n"); //~~~~~DEBUGGING~~~~~

            DoRequest(socketStream, connection); //Does the request received from client
        }

        static void DoRequest(NetworkStream stream, Socket connection)
        {
            try
            {
                StreamWriter sw = new StreamWriter(stream); //Writes to stream
                StreamReader sr = new StreamReader(stream); //Reads from stream

                ProtocolType type = ProtocolType.WHOIS; //Sets the protocol type to WHOIS by default

                string temp; //A temporary string variable
                string name = null; //A string used to hold the first line commonly containing the name
                List<string> header = new List<string>(); //A list of strings to add each line of optional headers to
                string location = null; //A string to save the location to 
                bool head = true; //A boolean to say whether the header is being read (true) or not (false)

                Stopwatch watch = new Stopwatch(); //Stopwatch for debugging. This allows for seeing how long it takes to do a segment of code

                int count = 1; //~~~~~DEBUGGING~~~~~

                if (debugMode) watch.Start(); //Starts the stopwatch to time the reading of the code //~~~~~DEBUGGING~~~~~

                try //Try and read
                {
                    while (true) //Loop through until break out
                    {
                        temp = sr.ReadLine(); //Set the temporary variable to the string that is read

                        if (debugMode && name == null)
                        {
                            Console.WriteLine("Received: \n");
                            Console.WriteLine("Line " + count + ": " + temp);
                            count++;
                        } //~~~~~DEBUGGING~~~~~
                        else if (debugMode)
                        {
                            Console.WriteLine("Line " + count + ": " + temp);
                            count++;
                        } //~~~~~DEBUGGING~~~~~

                        if (name == null) //If the first line hasn't been set yet...
                        {
                            name = temp; //... then set it to the temporary line
                        }
                        else if (head && (name.Contains("POST /") && name.Contains(" HTTP/1.0") || name.Contains("POST / HTTP/1.1")
                            || name.Contains("PUT /") || name.Contains("GET /?") && name.Contains(" HTTP/1.0")
                            || name.Contains("GET /?name=") && name.Contains(" HTTP/1.1"))) //Else if the header hasn't been read yet...
                        {
                            try //Try to...
                            {
                                if (string.IsNullOrWhiteSpace(temp)) //... and the temporary string is empty
                                {
                                    head = false; //Then we are no longer reading the header
                                }
                                else if (head && name.Contains("PUT /")) //Else if we are reading the header and the first line contains "PUT /"...
                                {
                                    location = temp; //... then set the location line to the temporary string
                                    break; //Break out of the for loop which means no more reading data
                                }
                                else //Else if the previous IFs aren't met then...
                                {
                                    header.Add(temp); //... add the temporary string to the list of header strings
                                }
                            }
                            catch //Catch any errors and... 
                            {
                                break;//... break out the reading loop
                            }
                        }
                        else if (head && temp == null)
                        {
                            break;
                        }
                        else //Else if the previous IFs aren't met then...
                        {
                            location = temp; //... then set the location 
                            break; //... break out the reading loop
                        }

                        if (!name.Contains("POST /") && !name.Contains("PUT /") && head) //If the name doesn't contain "POST /" and "PUT /" and it's currently reading the header then...
                        {
                            break; //... break out the reading loop
                        }
                    }
                }
                catch (IOException) //If it can't read anymore it will catch an IOException
                {
                    if (name != null) //If it has read some data
                    {
                        Console.WriteLine("\nERROR: Data was read but server/client timed out.\n");
                        status = "EXCEPTION";
                    }
                    else //Else if it hasn't read any data
                    {
                        Console.WriteLine("\nERROR: No data was retrieved.\n");
                        status = "EXCEPTION";
                    }
                }

                if (debugMode)
                {
                    Console.WriteLine("\n");
                    watch.Stop(); //Stop the stopwatch
                    Console.WriteLine("Time taken to read: " + watch.ElapsedMilliseconds + "ms");
                } //~~~~~DEBUGGING~~~~~

                if (string.IsNullOrWhiteSpace(location)) //If the location line is null...
                {
                    location = string.Join("\n", header); //... then the location line is all of the header lines joined together
                }

                if (name.Contains("POST / HTTP/1.1") || name.Contains("GET /?name=") && name.Contains(" HTTP/1.1")) //If the name contains the POST or GET with HTTP/1.1...
                {
                    type = ProtocolType.HTTP1; //... then the protocol type = HTTP1
                    if (debugMode) Console.WriteLine("Protocol: HTTP/1.1"); //~~~~~DEBUGGING~~~~~
                }
                else if ((name.Contains("GET /?") || name.Contains("POST /")) && name.Contains(" HTTP/1.0")) //Else if the name contains the POST or GET with HTTP/1.0...
                {
                    type = ProtocolType.HTTP0; //... then the protocol type = HTTP0
                    if (debugMode) Console.WriteLine("Protocol: HTTP/1.0"); //~~~~~DEBUGGING~~~~~
                }
                else if (name.Contains("GET /") || name.Contains("PUT /")) //Else if the name conatins the GET or PUT for the http0.9...
                {
                    type = ProtocolType.HTTP9; //... then the type = HTTP9
                    if (debugMode) Console.WriteLine("Protocol: HTTP/0.9"); //~~~~~DEBUGGING~~~~~
                    if (string.IsNullOrWhiteSpace(location) && name.Contains("PUT /"))
                    {
                        type = ProtocolType.WHOIS;
                        if (debugMode) Console.WriteLine("Protocol: WHOIS"); //~~~~~DEBUGGING~~~~~
                    }
                }
                else //Else if none of the previous IFs are met...
                {
                    type = ProtocolType.WHOIS; //... then the type = WHOIS
                    if (debugMode) Console.WriteLine("Protocol: WHOIS"); //~~~~~DEBUGGING~~~~~
                }

                switch (type) //Switch on the type to determine what to do for that computer
                {
                    case ProtocolType.HTTP1: //If the type is HTTP1 then...
                        string[] x = name.Split(new char[] { '=' }, 2); //Split the name string, into a string array, on the =

                        if (x[0].Contains("GET")) //If the first element contians GET..
                        {
                            string[] a = x[1].Split(new char[] { ' ' }, 2); //Split the second element of x into a string array

                            name = a[0]; //The name string now = the first element of a

                            if (locations.TryGetValue(name, out string value)) //If the database (dictionary) contains name
                            {
                                //Stream write this...
                                sw.WriteLine("HTTP/1.1" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n" + "\r\n" + value);
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine("HTTP/1.1" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n" + "\r\n" + value);
                                } //~~~~~DEBUGGING~~~~~
                            }
                            else //Otherwise...
                            {
                                //Stream write this...
                                sw.WriteLine("HTTP/1.1" + " " + "404" + " " + "Not" + " " + "Found" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine("HTTP/1.1" + " " + "404" + " " + "Not" + " " + "Found" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                                } //~~~~~DEBUGGING~~~~~
                                status = "UNKOWN";
                            }

                            line = "GET " + name;
                        }
                        else //Otherwise...
                        {
                            string[] a = location.Split(new char[] { '=' }, 3); //Split the location line by the =
                            string[] b = a[1].Split(new char[] { '&' }, 2); //Split the second element of a by the &

                            name = b[0]; //The name string now = the first element of b
                            location = a[2]; //The location string now = the third element of a

                            if (locations.TryGetValue(name, out string value)) //If the database (dictionary) contains name
                            {
                                locations[name] = location; //The Key's value now = location
                            }
                            else //Otherwise...
                            {
                                locations.Add(name, location); //Add the name and location to the dictionary
                            }

                            //Stream write this...
                            sw.WriteLine("HTTP/1.1" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                            if (debugMode)
                            {
                                Console.WriteLine("Reply: \n");
                                Console.WriteLine("HTTP/1.1" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                            } //~~~~~DEBUGGING~~~~~

                            line = "PUT " + name + " " + location;
                        }
                        break; //Break out of the case
                    case ProtocolType.HTTP0: //If the type is HTTP0 then...
                        string[] y = name.Split(new char[] { ' ' }, 3); //Split the name string by a space

                        if (y[0].Contains("GET")) //If the first element of y contains GET...
                        {
                            string[] a = y[1].Split(new char[] { '?' }, 2); //Split the second element of y on the ?

                            name = a[1]; //The name string now = the second element of a

                            if (locations.TryGetValue(name, out string value)) //If the database (dictionary) contains name
                            {
                                //Stream write this...
                                sw.WriteLine("HTTP/1.0" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n" + "\r\n" + value);
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine("HTTP/1.0" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n" + "\r\n" + value);
                                } //~~~~~DEBUGGING~~~~~
                            }
                            else //Otherwise...
                            {
                                //Stream write this...
                                sw.WriteLine("HTTP/1.0" + " " + "404" + " " + "Not" + " " + "Found" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine("HTTP/1.0" + " " + "404" + " " + "Not" + " " + "Found" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                                } //~~~~~DEBUGGING~~~~~
                                status = "UNKOWN";
                            }

                            line = "GET " + name;
                        }
                        else //Otherwise...
                        {
                            string[] a = y[1].Split(new char[] { '/' }, 2); //Split the second element of y on the /

                            name = a[1]; //The name string now = the second element of a

                            if (locations.TryGetValue(name, out string value)) //If the database (dictionary) contains name
                            {
                                locations[name] = location; //The Key's value is now = location
                            }
                            else //Otherwise...
                            {
                                locations.Add(name, location); //Add the name and location to the dictionary
                            }
                            //Stream write this...
                            sw.WriteLine("HTTP/1.0" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                            if (debugMode)
                            {
                                Console.WriteLine("Reply: \n");
                                Console.WriteLine("HTTP/1.0" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                            } //~~~~~DEBUGGING~~~~~

                            line = "PUT " + name + " " + location;
                        }
                        break; //Break out of the case
                    case ProtocolType.HTTP9: //If the type is HTTP9 then...
                        string[] z = name.Split(new char[] { '/' }, 2); //Split the name string, into a string array, on the /

                        if (z[0].Contains("GET ")) //If the first element of z contains GET
                        {
                            name = z[1]; //The name string now = the second element of z

                            if (locations.TryGetValue(name, out string value)) //If the database (dictionary) contains name
                            {
                                //Stream write this...
                                sw.WriteLine("HTTP/0.9" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n" + "\r\n" + value);
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine("HTTP/0.9" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n" + "\r\n" + value);
                                } //~~~~~DEBUGGING~~~~~
                            }
                            else //Otherwise...
                            {
                                //Stream write this...
                                sw.WriteLine("HTTP/0.9" + " " + "404" + " " + "Not" + " " + "Found" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine("HTTP/0.9" + " " + "404" + " " + "Not" + " " + "Found" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                                } //~~~~~DEBUGGING~~~~~
                                status = "UNKOWN";
                            }

                            line = "GET " + name;
                        }
                        else //Otherwise...
                        {
                            name = z[1]; //The name string now = the second element of z

                            if (locations.TryGetValue(name, out string value)) //If the database (dictionary) contains name
                            {
                                locations[name] = location; //The Key's value now = location
                            }
                            else //Otherwise...
                            {
                                locations.Add(name, location); //Add the name and location to the dictionary
                            }

                            //Stream write this...
                            sw.WriteLine("HTTP/0.9" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                            if (debugMode)
                            {
                                Console.WriteLine("Reply: \n");
                                Console.WriteLine("HTTP/0.9" + " " + "200" + " " + "OK" + "\r\n" + "Content-Type:" + " " + "text/plain" + "\r\n");
                            } //~~~~~DEBUGGING~~~~~

                            status = "OK";
                            line = "PUT " + name + " " + location;
                        }
                        break; //Break out of the case
                    case ProtocolType.WHOIS: //If the type is WHOIS then...
                        string[] args = name.Split(new char[] { ' ' }, 2); //Reads the line and splits it on any spaces.

                        //If there are two args then it is an UPDATE.
                        if (args.Length == 2)
                        {
                            if (locations.TryGetValue(args[0], out string value)) //If the database (dictionary) contains name
                            {
                                //If the key is found then it's corresponding value is changed.
                                locations[args[0]] = args[1];
                            }
                            else
                            {
                                //If the key is not found then it is added with it's corresponding value.
                                locations.Add(args[0], args[1]);
                            }

                            //Stream write this...
                            sw.WriteLine("OK"); //Returns OK to the client.
                            if (debugMode)
                            {
                                Console.WriteLine("Reply: \n");
                                Console.WriteLine("OK");
                            } //~~~~~DEBUGGING~~~~~

                            status = "OK";
                            line = "PUT " + name + " " + location;
                        }
                        //If there is one arg then it is a LOOKUP.
                        else
                        {
                            if (locations.TryGetValue(args[0], out string value)) //If the database (dictionary) contains name
                            {
                                //If the key is found then it's corresponding value is returned.
                                //Stream write this...
                                sw.WriteLine(value);
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine(value);
                                } //~~~~~DEBUGGING~~~~~
                            }
                            else //Otherwise...
                            {
                                //If the key isn't found then there is no value to return.
                                //Stream write this...
                                sw.WriteLine("ERROR: no entries found");
                                if (debugMode)
                                {
                                    Console.WriteLine("Reply: \n");
                                    Console.WriteLine("ERROR: no entries found");
                                } //~~~~~DEBUGGING~~~~~
                                status = "UNKOWN";
                            }

                            line = "GET " + name;
                        }
                        break; //Break out of the case
                }

                sw.Flush(); //Empties the buffer
            }
            catch (Exception e)
            {
                if (debugMode) Console.WriteLine("This should never be reached! An error must have occurred.");
                if (debugMode) Console.WriteLine(e.ToString());
            }
            finally
            {
                if (debugMode) Console.WriteLine("Connection Terminated.\n~~~~~~~~~~~~~~~~~~~~~~~~~");//~~~~~DEBUGGING~~~~~

                stream.Close(); //Closes NetworkStream
                connection.Close(); //Closes the connection

                log.WriteLog(hostName, line, status);
                database.WriteDatabase(locations);
                database.SaveDatabase();
            }
        }

    }

    //Logging example from http://stackoverflow.com/questions/2954900/simple-multithread-safe-log-class
    public class MTLogging
    {
        public static string fileName = null;

        public MTLogging(string p_fileName)
        {
            fileName = p_fileName;
        }

        private static readonly object locker = new object();

        public void WriteLog(string hostname, string message, string status)
        {
            string line = hostname + " - - " + DateTime.Now.ToString("'['dd'/'MM'/'yyyy':'HH':'mm':'ss zz00']'") + " \"" + message + "\" " + status;

            lock (locker)
            {
                Console.WriteLine(line);
                if (fileName == null) return;
                try
                {
                    StreamWriter sw;
                    sw = File.AppendText(fileName);
                    sw.WriteLine(line);
                    sw.Close();
                }
                catch
                {
                    Console.WriteLine("Unable to write to log file: " + fileName);
                }
            }
        }
    }

    public class Database
    {
        /*public static Dictionary<string, string> locations;
        public static string databaseName = null;
        private static readonly object locker = new object();

        public Database(string p_databaseName)
        {
            databaseName = p_databaseName;
        }

        public Dictionary<string,string> ReadDatabase()
        {
            lock (locker)
            {
                if (databaseName == null) return new Dictionary<string, string>();
                try
                {
                    while (true)
                    {
                        try
                        {
                            StreamReader sr = new StreamReader(databaseName);
                            string temp = sr.ReadLine();
                            if (string.IsNullOrWhiteSpace(temp))
                            {
                                break;
                            }
                            else
                            {
                                string[] nameLocation = temp.Split(new char[] { ',' }, 2);
                                locations.Add(nameLocation[0], nameLocation[1]);
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Unable to write to database file: " + databaseName);
                }
            }
            return locations;
        }

        public void WriteDatabase(Dictionary<string,string> p_locations)
        {
            locations = p_locations;
            string line = null;

            lock (locker)
            {
                if (databaseName == null) return;
                try
                {
                    StreamWriter sw = new StreamWriter(databaseName);
                    foreach (KeyValuePair<string,string> pair in locations)
                    {
                        line = pair.Key + ',' + pair.Value;
                        sw.WriteLine(line);
                    }
                }
                catch
                {
                    Console.WriteLine("Unable to write to database file: " + databaseName);
                }
            }
        }*/

        public static Dictionary<string, string> locations = new Dictionary<string, string>();
        public static string filename = null;

        public Database(string p_filename)
        {
            filename = p_filename;
        }

        public Dictionary<string, string> ReadDatabase()
        {
            return locations;
        }

        public void WriteDatabase(Dictionary<string, string> p_locations)
        {
            foreach (KeyValuePair<string, string> pair in p_locations)
            {
                if (locations.TryGetValue(pair.Key, out string value))
                {
                    if (value != pair.Value)
                    {
                        locations[pair.Key] = pair.Value;
                    }
                }
                else
                {
                    locations.Add(pair.Key, pair.Value);
                }
            }
        }

        private static readonly object locker = new object();

        public void LoadDatabase()
        {
            Dictionary<string, string> temp = new Dictionary<string, string>();

            if (filename == null) return;

            lock (locker)
            {
                try
                {
                    string[] line = File.ReadAllLines(filename);

                    for (int i = 0; i < line.Length; i++)
                    {
                        string[] s;
                        if (line[i].Contains(","))
                        {
                            s = line[i].Split(new char[] { ',' }, 2);
                            temp.Add(s[0], s[1]);
                        }
                    }
                    WriteDatabase(temp);
                }
                catch
                {
                    Console.WriteLine("Unable to load the database file: " + filename);
                }
            }
        }

        public void SaveDatabase()
        {
            if (filename == null) return;

            lock (locker)
            {
                try
                {
                    Dictionary<string, string> temp = new Dictionary<string, string>();
                    try
                    {
                        string[] lines = File.ReadAllLines(filename);

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string[] s = lines[i].Split(new char[] { ',' }, 2);
                            temp.Add(s[0], s[1]);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Unable to load the database file: " + filename);
                    }
                    //StreamWriter sw;
                    //sw = File.AppendText(filename);

                    List<string> line = new List<string>();

                    foreach (string entry in locations.Keys)
                    {
                        line.Add(entry + "," + locations[entry] + "\r\n");
                    }

                    File.WriteAllText(filename, string.Join("\r", line));
                    Console.WriteLine(string.Join("\r", line));

                    //sw.WriteLine(line);
                    //sw.Close();
                }
                catch
                {
                    Console.WriteLine("Unable to write to database file: " + filename);
                }
            }
        }
    }
}
