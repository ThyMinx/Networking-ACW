using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.IO;

namespace Location
{
    /// <summary>
    /// Interaction logic for Interface.xaml
    /// </summary>
    public partial class Interface : Window
    {
        static bool debugMode = false;
        static ProtocolType type = ProtocolType.WHOIS;
        static TcpClient client;
        static List<string> inputs;
        static string server;

        enum ProtocolType
        {
            WHOIS,
            HTTP9,
            HTTP0,
            HTTP1
        }

        public Interface()
        {
            InitializeComponent();
        }

        private void B_Run_Click_1(object sender, RoutedEventArgs e)
        {
            t_output.Text = "";
            string name = null;
            string location = null;
            client = new TcpClient(); //Creating a client object. (Called client for ease of understanding)

            //Connect method is part of Socket Library.
            //This section connects the client to a server.
            string defaultServer = "whois.net.dcs.hull.ac.uk";
            server = null;

            int defaultPort = 43;
            int port = 0;

            int timeout = 1000;

            ProtocolType type = ProtocolType.WHOIS;

            inputs = new List<string>();

            if (cb_debug.IsChecked == true)
            {
                debugMode = true;
                t_output.Text += "Debug mode enabled.\n";
            }
            else
            {
                debugMode = false;
            }

            try
            {
                port = Int32.Parse(t_Port.Text);
                if (debugMode) t_output.Text += "Port: " + port + "\n";
            }
            catch
            {
                port = defaultPort;
                if (debugMode) t_output.Text += "No port given. Using default port: " + defaultPort + "\n";
            }

            try
            {
                timeout = Int32.Parse(t_timeout.Text);
                if (debugMode) t_output.Text += "Timeout time: " + timeout + "ms\n";
            }
            catch
            {
                if (debugMode) t_output.Text += "No timeout time given. Using default timeout time: " + timeout + "ms\n";
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(t_Host.Text))
                {
                    server = t_Host.Text;
                }
                if (debugMode) t_output.Text += "Hostname: " + server + "\n";
            }
            catch
            {
                server = "whois.net.dcs.hull.ac.uk";
                if (debugMode) t_output.Text += "No hostname given. Using default hostname: " + defaultServer + "\n";
            }

            if (!string.IsNullOrWhiteSpace(t_name.Text))
            {
                name = t_name.Text;
                inputs.Add(name);
            }

            if (!string.IsNullOrWhiteSpace(t_location.Text))
            {
                location = t_location.Text;
                inputs.Add(location);
            }

            if (inputs.Count == 0)
            {
                if (debugMode) t_output.Text += "No arguments given.\n";
            }
            else
            {
                try
                {
                    client.Connect(server, port); //Connects to the client on the HOSTNAME and PORT

                    if (timeout != 0)
                    {
                        client.ReceiveTimeout = 1000; //Sets the timeout
                        client.SendTimeout = 1000; //Sets the timeout
                    }

                    Write(); //Do the write method
                    Read(); //Do the read method
                }
                catch
                {
                    t_output.Text = "Cannot connect to server.";
                }
            }
        }

        private void c_Protocols_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (c_Protocols.Text)
            {
                case "HTTP/0.9":
                    type = ProtocolType.HTTP9;
                    break;
                case "HTTP/1.0":
                    type = ProtocolType.HTTP0;
                    break;
                case "HTTP/1.1":
                    type = ProtocolType.HTTP1;
                    break;
                default:
                    type = ProtocolType.WHOIS;
                    break;
            }
        }
        public void Read()//TcpClient client, List<string> inputs, ProtocolType type)
        {
            StreamReader sr = new StreamReader(client.GetStream()); //Reads from stream.

            string answer = ""; //This is where all the data gets stored into one string.
            string temp; //This is a temporary string that gets rewritten for each line read.
            List<string> lines = new List<string>(); //This list is a list of strings, each element is a line read in.
            try //Read
            {
                while ((temp = sr.ReadLine()) != null) //If the read line is null don't continue else do...
                {
                    lines.Add(temp); //Add to the lines list of strings. 
                }
                answer = string.Join("\n", lines); //Joing all the strings into one.
            }
            catch (IOException) //If it can't read anymore it will catch an IOException
            {
                if (lines.Count > 0) //If it has read some data.
                {
                    answer = string.Join("\n", lines); //Join all the strings into one.
                    if (debugMode) t_output.Text += "Data received: " + answer + "\n";
                }
                else
                {
                    answer = "Client timed out."; //If no data was read then the client timed out.
                    t_output.Text += answer + "\n";
                    return;
                }
            }

            string[] args = answer.Split(new char[] { '\n' }); //Splits the string up into a list of args based on each new line.

            if (args[0].Contains("404") && type != ProtocolType.WHOIS) //If the first line back contains 404 it means that there was an error.
            {
                t_output.Text += "ERROR: no entries found" + "\n";
            }
            else
            {
                switch (type) //Depending on the protocol type depends on the protocol sequence read.
                {
                    case ProtocolType.HTTP1: //For HTTP1.1
                        try
                        {
                            if (args[0].Contains("OK"))
                            {
                                if (inputs.Count == 2) //If there were two inputs then the location has been UPDATED
                                {
                                    t_output.Text += inputs[0] + " location changed to be " + inputs[1] + "\n";
                                }
                                else //Else the location is location.
                                {
                                    List<string> header = new List<string>(); //A list of strings which will be full of all the header.
                                    List<string> locationList = new List<string>(); //A list of strings which will be full of the rest, which should be the location.
                                    bool head = true; //This boolean is while it is reading the header it = true
                                    for (int i = 0; i < lines.Count; i++)
                                    {
                                        if (string.IsNullOrWhiteSpace(args[i]) && head) //If it's still reading the header and the line it's trying to read is null or whitespace then the header has finished.
                                        {
                                            head = false; //Reading the header is now = false
                                        }
                                        else if (head) //If it is currently reading the header
                                        {
                                            header.Add(lines[i]); //Add this line being read to the header
                                        }

                                        if (!head) //If it isn't reading the header
                                        {
                                            locationList.Add(lines[i]); //Add this line to the locationList
                                        }
                                    }

                                    string location = string.Join("\n", locationList); //The location is every line in locationList so join them together.

                                    t_output.Text += inputs[0] + " is " + location.Trim() + "\n";
                                }
                            }
                        }
                        catch
                        {
                            type = ProtocolType.HTTP0;
                        }
                        break;
                    case ProtocolType.HTTP0: //For HTTP1.0
                        try
                        {
                            if (args[0].Contains("OK"))
                            {
                                if (inputs.Count == 2) //If there were two inputs then the location has been UPDATED
                                {
                                    t_output.Text += inputs[0] + " location changed to be " + inputs[1] + "\n";
                                }
                                else //Else the location is location.
                                {
                                    string location = null; //Location = null.
                                    for (int i = 0; i < lines.Count; i++)
                                    {
                                        if (string.IsNullOrWhiteSpace(args[i])) //If the current line being read = null
                                        {
                                            location = args[i + 1]; //Location = the next line.
                                            break; //Break out of the loop.
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    t_output.Text += inputs[0] + " is " + location.Trim() + "\n";
                                }
                            }
                        }
                        catch
                        {
                            type = ProtocolType.HTTP9;
                        }
                        break;
                    case ProtocolType.HTTP9: //For HTTP0.9
                        try
                        {
                            if (args[0].Contains("OK"))
                            {
                                if (inputs.Count == 2) //If there were two inputs then the location has been UPDATED
                                {
                                    t_output.Text += inputs[0] + " location changed to be " + inputs[1] + "\n";
                                }
                                else //Else the location is location.
                                {
                                    string location = null; //Location = null.
                                    for (int i = 0; i < lines.Count; i++)
                                    {
                                        if (string.IsNullOrWhiteSpace(args[i])) //If the current line being read = null
                                        {
                                            try
                                            {
                                                location = args[i + 1]; //Location = the next line.
                                            }
                                            catch
                                            {
                                                location = "this is the problem";
                                            }
                                            break; //Break out of the loop.
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    t_output.Text += inputs[0] + " is " + location.Trim() + "\n";
                                }
                            }
                        }
                        catch
                        {
                            type = ProtocolType.WHOIS;
                        }
                        break;
                    case ProtocolType.WHOIS: //For the default WHOIS
                        try
                        {
                            if (args.Length > 1)
                            {
                                if (args[0].Contains("OK"))
                                {
                                    if (inputs.Count == 2) //If there were two inputs then the location has been UPDATED
                                    {
                                        t_output.Text += inputs[0] + " location changed to be " + inputs[1] + "\n";
                                    }
                                    else //Else the location is location.
                                    {
                                        string location = null; //Location = null.
                                        for (int i = 0; i < lines.Count; i++)
                                        {
                                            if (string.IsNullOrWhiteSpace(args[i])) //If the current line being read = null
                                            {
                                                try
                                                {
                                                    location = args[i + 1]; //Location = the next line.
                                                }
                                                catch
                                                {
                                                    //location = "this is the problem";
                                                }
                                                break; //Break out of the loop.
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        t_output.Text += inputs[0] + " is " + location.Trim() + "\n";
                                    }
                                }
                                break;
                            }
                            switch (answer)
                            {
                                case "OK\r\n":
                                    if (inputs.Count == 2)
                                    {
                                        t_output.Text += inputs[0] + " location changed to be " + inputs[1] + "\n";
                                    }
                                    else
                                    {
                                        t_output.Text += inputs[0] + " is " + answer.Trim() + "\n";
                                    }
                                    break;
                                case "OK":
                                    if (inputs.Count == 2)
                                    {
                                        t_output.Text += inputs[0] + " location changed to be " + inputs[1] + "\n";
                                    }
                                    else
                                    {
                                        t_output.Text += inputs[0] + " is " + answer.Trim() + "\n";
                                    }
                                    break;
                                case "ERROR: no entries found\r\n":
                                    t_output.Text += answer.Trim() + "\n";
                                    break;
                                case "ERROR: no entries found":
                                    t_output.Text += answer.Trim() + "\n";
                                    break;
                                default:
                                    t_output.Text += inputs[0] + " is " + answer.Trim() + "\n";
                                    break;
                            }
                        }
                        catch
                        {
                            type = ProtocolType.HTTP1;
                        }
                        break;
                }
            }

        }

        public void Write()
        {
            StreamWriter sw = new StreamWriter(client.GetStream()); //Writes to stream.

            //Writing depending on the protocol
            switch (type)
            {
                case ProtocolType.HTTP1:
                    switch (inputs.Count)
                    {
                        case 1: //If there is only one argument it should LOOKUP the location of a student.
                            sw.WriteLine("GET" + " " + "/?" + "name=" + inputs[0] + " " + "HTTP/1.1");
                            sw.WriteLine("Host: " + server);
                            sw.WriteLine(""); //The inputs[0] is whatever the user typed.
                            if (debugMode) t_output.Text += "GET /?name=" + inputs[0] + " HTTP/1.1\r\nHost: " + server + "\r\n" + "\n";
                            break;
                        case 2: //If there are two arguments it should UPDATE the location.
                            int length = inputs[0].ToCharArray().Length + inputs[1].ToCharArray().Length + 15;

                            sw.WriteLine("POST" + " " + "/" + " " + "HTTP/1.1");
                            sw.WriteLine("Host: " + server);
                            sw.WriteLine("Content-Length:" + " " + length.ToString());
                            sw.WriteLine("");
                            sw.WriteLine("name=" + inputs[0] + "&" + "location=" + inputs[1]); //The inputs[0] and inputs[1] is whatever the user typed.
                            if (debugMode) t_output.Text += "POST / HTTP/1.1\r\nHost: " + server + "\r\nContent-Length: " + length.ToString() + "\r\n\r\nname=" + inputs[0] + "&location=" + inputs[1] + "\n";
                            break;
                        default: //If arguments are too many or too little.
                            if (debugMode) t_output.Text += "Wrong amount of arguments!" + "\n";
                            break;
                    }
                    break;
                case ProtocolType.HTTP0:
                    switch (inputs.Count)
                    {
                        case 1: //If there is only one argument it should LOOKUP the location of a student.
                            sw.WriteLine("GET" + " " + "/?" + inputs[0] + " " + "HTTP/1.0");
                            sw.WriteLine(""); //The inputs[0] is whatever the user typed.
                            if (debugMode) t_output.Text += "GET /?" + inputs[0] + " HTTP/1.0\r\n" + "\n";
                            break;
                        case 2: //If there are two arguments it should UPDATE the location.
                            sw.WriteLine("POST" + " " + "/" + inputs[0] + " " + "HTTP/1.0");
                            sw.WriteLine("Content-Length:" + " " + inputs[1].ToCharArray().Length.ToString());
                            sw.WriteLine("");
                            sw.WriteLine(inputs[1]); //The inputs[0] and inputs[1] is whatever the user typed.
                            if (debugMode) t_output.Text += "POST /" + inputs[0] + " HTTP/1.0\r\nContent-Length: " + inputs[1].ToCharArray().Length.ToString() + "\r\n\r\n" + inputs[1] + "\n";
                            break;
                        default: //If arguments are too many or too little.
                            if (debugMode) t_output.Text += "Wrong amount of arguments!" + "\n";
                            break;
                    }
                    break;
                case ProtocolType.HTTP9:
                    switch (inputs.Count)
                    {
                        case 1: //If there is only one argument it should LOOKUP the location of a student.
                            sw.WriteLine("GET" + " " + "/" + inputs[0]); //The inputs[0] is whatever the user typed.
                            if (debugMode) t_output.Text += "GET /" + inputs[0] + "\n";
                            break;
                        case 2: //If there are two arguments it should UPDATE the location.
                            sw.WriteLine("PUT" + " " + "/" + inputs[0]);
                            //sw.WriteLine();
                            sw.WriteLine(inputs[1]); //The inputs[0] and inputs[1] is whatever the user typed.
                            if (debugMode) t_output.Text += "PUT /" + inputs[0] + "\r\n\r\n" + inputs[1] + "\n";
                            break;
                        default: //If arguments are too many or too little.
                            if (debugMode) t_output.Text += "Wrong amount of arguments!" + "\n";
                            break;
                    }
                    break;
                case ProtocolType.WHOIS:
                    switch (inputs.Count)
                    {
                        case 1: //If there is only one argument it should LOOKUP the location of a student.
                            sw.WriteLine(inputs[0]); //The inputs[0] is whatever the user typed.
                            if (debugMode) t_output.Text += inputs[0] + "\n";
                            break;
                        case 2: //If there are two arguments it should UPDATE the location.
                            sw.WriteLine(inputs[0] + " " + inputs[1]); //The inputs[0] and inputs[1] is whatever the user typed.
                            if (debugMode) t_output.Text += inputs[0] + " " + inputs[1] + "\n";
                            break;
                        default: //If arguments are too many or too little.
                            if (debugMode) t_output.Text += "Wrong amount of arguments!" + "\n"; //If the wrong amount of arguments are supplied.
                            break;
                    }
                    break;
            }

            sw.Flush(); //Empties the buffer.
        }

    }
}
