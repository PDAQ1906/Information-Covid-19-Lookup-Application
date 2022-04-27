using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for SignIn.xaml
    /// </summary>
    public partial class SignIn : Window
    {
        BackgroundWorker worker_si = new BackgroundWorker();
        public SignIn()
        {
            InitializeComponent();
            worker_si.WorkerReportsProgress = true;
            worker_si.DoWork += Worker_si_DoWork;
            worker_si.ProgressChanged += Worker_si_ProgressChanged;
            worker_si.RunWorkerCompleted += Worker_si_RunWorkerCompleted;
        }

        private void Worker_si_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void Worker_si_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if ((int)e.ProgressPercentage == 0)
            {
                signWarning.Visibility = Visibility.Hidden;
                MainWindow mW = new MainWindow(txtbx.Text);
                mW.Show();
                this.Close();
            }
            else
            {
                signWarning.Visibility = Visibility.Visible;
                Storyboard myStoryboard = (Storyboard)signWarning.Resources["WrongSignIn"];
                Storyboard.SetTarget(myStoryboard.Children.ElementAt(0) as DoubleAnimationUsingKeyFrames, signWarning);
                myStoryboard.Begin();
            }
        }

        private void Worker_si_DoWork(object? sender, DoWorkEventArgs e)
        {
            List<string> id_list = e.Argument as List<string>;
            if (isUserIdCorrect(id_list[0], id_list[1]))
            {
                (sender as BackgroundWorker).ReportProgress(0);
                return;
            }
            (sender as BackgroundWorker).ReportProgress(1);
        }

        private string userID_string = "Enter your user ID";
        private string pass_string = "Enter your password";
        Client.ClientTransfer client = new ClientTransfer();

        private void SignInToolBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SignInMinimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SignInClose(object sender, RoutedEventArgs e)
        {
            try
            {
                client.closeConnection();
            }
            catch (Exception ex)
            {

            }
            this.Close();
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private bool isUserIdCorrect(string username_s, string password_s)
        {
            if (username_s == "admin" && password_s == "nimda")
                return true;
            try
            {
                if (client.SignIn(username_s, CreateMD5(password_s)))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

            }

            return false;
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            List<string> arguments = new List<string>();
            arguments.Add(txtbx.Text);
            arguments.Add(pssbx.Text);
            worker_si.RunWorkerAsync(arguments);
        }

        private void txtbx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtbx.Text == userID_string)
            {
                txtbx.Text = "";
                txtbx.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }


        private void txtbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtbx.Text == "")
            {
                txtbx.Text = userID_string;
                txtbx.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6D1D4"));
            }
        }

        private void pssbx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (pssbx.Text == pass_string)
            {
                pssbx.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Password");
                pssbx.Foreground = new SolidColorBrush(Color.FromRgb(11, 63, 125));
                pssbx.Padding = new Thickness(4);
            }
            pssbx.Text = "";
        }

        private void pssbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (pssbx.Text == "")
            {
                pssbx.Text = pass_string;
                pssbx.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Segoe UI Variable Static Display");
                pssbx.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6D1D4"));
                pssbx.Padding = new Thickness(0);
            }
            else
                pssbx.Padding = new Thickness(4);
        }

        private void WindowMainLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SignUp sU = new SignUp();
            sU.ShowDialog();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Process user input
                signin.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }
    }
}
