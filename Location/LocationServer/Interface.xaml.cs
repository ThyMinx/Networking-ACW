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
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace LocationServer
{
    /// <summary>
    /// Interaction logic for Interface.xaml
    /// </summary>
    public partial class Interface : Window
    {
        static bool debug = false;
        static MTLogging log = null;
        static Database data = null;
        static int timeout = 1000;

        public Interface(bool d, MTLogging lo, Database da)
        {
            InitializeComponent();
            debug = d;
            log = lo;
            data = da;

            debug = true;

            log = new MTLogging(t_logfile.Text);
            Int32.TryParse(t_timeout.Text, out timeout);
            data = new Database(t_database.Text);
            if (t_database.Text != null) data.LoadDatabase();
        }
        private void t_timeout_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void t_logfile_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void B_Run_Click_1(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(DoStuff));
        }

        private void t_database_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        void DoStuff(object state)
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 43);
                listener.Start();
                while (true)
                {
                    Socket connection = listener.AcceptSocket();
                    Server server = new Server(); //Creates a new object of server

                    Thread t = new Thread(() => server.RunServer(connection, debug, log, data)); //Creates a new thread running the server object with it's method
                    t.IsBackground = true;
                    t.Start(); //Starts the thread
                }
            }
            catch
            {

            }
        }

        /*public void Output_Update(string output)
        {
            t_output.Text = output;
        }*/
    }
}
