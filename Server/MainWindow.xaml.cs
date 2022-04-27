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
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Leaf.xNet;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public static Boolean main_running = true;
        private bool isIpv4(string ipString)
        {
            for (int i = 0; i < ipString.Length; i++)
            {
                if (ipString[i] != '.' && (ipString[i] > '9' || ipString[i] < '0'))
                    return false;
            }
            return true;
        }

        public List<ActionTracking> items = new List<ActionTracking>();
        public ActionTracking actionTracking = new ActionTracking();
        public MainWindow()
        {
            InitializeComponent();
            //lvUsers.ItemsSource = items;
            //items.Add(new ActionTracking() { IP = "192.168.56.1", Port = 99323, Action = "Client searched for 'Vietnam'." });
            //items.Add(new ActionTracking() { IP = "Jane Doe", Port = 39, Action = "jane@doe-family.com" });
            //lvUsers.ItemsSource = items;
            Thread updateData = new Thread(() =>
            {
                while (main_running)
                {
                    ServerTransfer.UpdateData();
                }
                return;
            });
            updateData.Name = "UpdateData";
            updateData.Start();

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[1];

            for (int i = 1; i < ipHost.AddressList.Length; i += 2)
            {
                if (isIpv4(ipHost.AddressList[i].ToString()))
                {
                    ipAddr = ipHost.AddressList[i];
                    IpName.Text = ipHost.AddressList[i].ToString();
                    break;
                }
            }
            //Console.WriteLine(ipAddr.ToString());

            TcpListener listener = new TcpListener(ipAddr, Server.ServerTransfer.PORT_NUMBER);
            int currentConnections = 0;
            int maxConnections = 5;
            connection_status.Dispatcher.Invoke(new Action(() =>
            {
                connection_status.Text = currentConnections.ToString() + "/" + maxConnections.ToString();
            }));
            // 1. listen
            listener.Start();
            Thread main_thread = new Thread(() =>
            {
                while ((currentConnections == 0 || currentConnections <= maxConnections) && main_running)
                {
                    // Connect
                    Socket socket = listener.AcceptSocket();
                    lvUsers.Dispatcher.Invoke(new Action(() =>
                    {
                        lvUsers.Items.Add(new ActionTracking()
                        {
                            Date = DateTime.Now.ToString("HH:mm:ss, dd/MM/yyyy"),
                            IP = (((IPEndPoint)socket.RemoteEndPoint).Address).ToString(),
                            Port = (((IPEndPoint)socket.RemoteEndPoint).Port).ToString(),
                            Action = "Client connected."
                        });
                    }));
                    currentConnections++;
                    connection_status.Dispatcher.Invoke(new Action(() =>
                    {
                        connection_status.Text = currentConnections.ToString() + "/" + maxConnections.ToString();
                    }));
                    Thread t = new Thread((obj) =>
                    {

                        while (main_running)
                        {
                            int checkConnections = ServerTransfer.RespondClient((Socket)obj, ref actionTracking);

                            lvUsers.Dispatcher.Invoke(new Action(() =>
                            {
                                lvUsers.Items.Add(new ActionTracking()
                                {
                                    Date = actionTracking.Date,
                                    IP = actionTracking.IP,
                                    Port = actionTracking.Port,
                                    Action = actionTracking.Action
                                });
                            }));
                            if (checkConnections == 1)
                            {
                                checkConnections--;
                                currentConnections--;
                                connection_status.Dispatcher.Invoke(new Action(() =>
                                {
                                    connection_status.Text = currentConnections.ToString() + "/" + maxConnections.ToString();
                                }));
                                break;
                            }
                        }
                        return;

                    });
                    t.IsBackground = true;
                    t.Start(socket);
                }
                listener.Stop();
                return;

            });
            main_thread.Name = "MainThreadProcess";
            main_thread.IsBackground = true;
            main_thread.Start();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            main_running = false;
            this.Close();
        }

        private void MainWindowToolBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            main_running = false;
            Environment.Exit(0);
        }

        private void Copy_IP_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText((string)IpName.Text);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            main_running = false;
            Environment.Exit(0);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            lvUsers.Items.Clear();
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow iw = new InfoWindow();
            iw.Show();
        }
    }

    public class ActionTracking
    {
        public string Date { get; set; }

        public string IP { get; set; }

        public string Port { get; set; }

        public string Action { get; set; }
    }
}
