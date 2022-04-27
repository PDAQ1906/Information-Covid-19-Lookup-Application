using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for IpRequest.xaml
    /// </summary>
    public partial class IpRequest : Window
    {
            BackgroundWorker worker = new BackgroundWorker();
        public IpRequest()
        {
            InitializeComponent();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        // Declarations
        public string ip_string = "Enter your server IP";

        private void IPToolBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void sv_txtbx_GotFocus(object sender, RoutedEventArgs e)
        {
            if(sv_txtbx.Text == ip_string)
            {
                sv_txtbx.Text = "";
                sv_txtbx.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        private void sv_txtbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sv_txtbx.Text == "")
            {
                sv_txtbx.Text = ip_string;
                sv_txtbx.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6D1D4"));
            }
        }


        private bool checkIpMatch(string ip_adress)
        {
            if(ip_adress == "1.22.333.4444") // Test IP
            {
                return true;
            }

            Client.ClientTransfer client;
            try
            {
                client = new Client.ClientTransfer(ip_adress);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IpWarning.Visibility = Visibility.Hidden;
            if (!worker.IsBusy)
            { 
                connect_but.Content = "Connecting...";
                worker.RunWorkerAsync(sv_txtbx.Text);
            }
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
           
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            
            if (e.ProgressPercentage == 0)
            {
                connect_but.Content = "Connect";
                sv_txtbx.BorderBrush = new SolidColorBrush(Color.FromRgb(198, 209, 212));
                IpWarning.Visibility = Visibility.Visible;
                Storyboard myStoryboard = (Storyboard)IpWarning.Resources["WrongServerIp"];
                Storyboard.SetTarget(myStoryboard.Children.ElementAt(0) as DoubleAnimationUsingKeyFrames, IpWarning);
                myStoryboard.Begin();
            }
            else
            {
                IpWarning.Visibility = Visibility.Hidden;
                sv_txtbx.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c6d1d4"));
                SignIn sU = new SignIn();
                sU.Show();
                this.Close();
            }
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            object ip_match = checkIpMatch((string)e.Argument);
            if ((bool)ip_match)
                (sender as BackgroundWorker).ReportProgress(1);
            else
                (sender as BackgroundWorker).ReportProgress(0);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Process user input
                connect_but.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }
}
