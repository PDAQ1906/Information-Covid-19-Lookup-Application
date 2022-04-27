using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
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
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Window
    {
        BackgroundWorker worker_su = new BackgroundWorker();
        public SignUp()
        {
            InitializeComponent();
            worker_su.WorkerReportsProgress = true;
            worker_su.DoWork += Worker_DoWork; ;
            worker_su.ProgressChanged += Worker_ProgressChanged; ;
            worker_su.RunWorkerCompleted += Worker_RunWorkerCompleted; ;

        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            int value = (int)e.ProgressPercentage;
            if (value == 0)
            {
                PassStrengthWarning.Visibility = Visibility.Visible;
                PassWarning.Visibility = Visibility.Hidden;
                IdWarning.Visibility = Visibility.Hidden;
                Storyboard myStoryboard = (Storyboard)signUpWarning.Resources["WrongSignUp"];
                Storyboard.SetTarget(myStoryboard.Children.ElementAt(0) as DoubleAnimationUsingKeyFrames, signUpWarning);
                myStoryboard.Begin();
                return;
            }

            if (value == 1)
            {
                PassStrengthWarning.Visibility = Visibility.Hidden;
                IdWarning.Visibility = Visibility.Hidden;
                PassWarning.Visibility = Visibility.Visible;
                Storyboard myStoryboard = (Storyboard)signUpWarning.Resources["WrongSignUp"];
                Storyboard.SetTarget(myStoryboard.Children.ElementAt(0) as DoubleAnimationUsingKeyFrames, signUpWarning);
                myStoryboard.Begin();
                return;
            }

            if (value == 2)
            {
                PassStrengthWarning.Visibility = Visibility.Hidden;
                PassWarning.Visibility = Visibility.Hidden;
                IdWarning.Visibility = Visibility.Visible;
                Storyboard myStoryboard = (Storyboard)signUpWarning.Resources["WrongSignUp"];
                Storyboard.SetTarget(myStoryboard.Children.ElementAt(0) as DoubleAnimationUsingKeyFrames, signUpWarning);
                myStoryboard.Begin();
                return;
            }

            if (value == 3)
            {
                PassStrengthWarning.Visibility = Visibility.Hidden;
                IdWarning.Visibility = Visibility.Hidden;
                PassWarning.Visibility = Visibility.Hidden;
                this.Focus();
                MessageWindow message = new MessageWindow("Signed up successfully.");
                message.Title = "Notifications";
                message.ShowDialog();
                this.Close();
            }
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

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            List<string> id_list = e.Argument as List<string>;
            if (!checkStrongPasswordMatch(id_list[1]))
            {
                (sender as BackgroundWorker).ReportProgress(0);
                return;
            }

            if (!checkPasswordMatch(id_list[1], id_list[2]))
            {
                (sender as BackgroundWorker).ReportProgress(1);
                return;
            }

            if (checkIdDuplicate(id_list[0], CreateMD5(id_list[1])))
            {
                (sender as BackgroundWorker).ReportProgress(2);
                return;
            }

            // return 3
            (sender as BackgroundWorker).ReportProgress(3);
            //
            
        }

        private void SignUpToolBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SignUpMinimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void SignUpClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.GetType() == typeof(SignIn))
                {
                    (window as SignIn).Focus();
                }
            }
        }

        private string userID_string = "Enter your user ID";
        private string pass_string = "Enter your password";
        Client.ClientTransfer client = new ClientTransfer();

        private void su_txtbx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (su_txtbx.Text == userID_string)
            {
                su_txtbx.Text = "";
                su_txtbx.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
        }

        private void su_txtbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (su_txtbx.Text == "")
            {
                su_txtbx.Text = userID_string;
                su_txtbx.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6D1D4"));
            }
        }

        private void su_pssbx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (su_pssbx.Text == pass_string)
            {
                su_pssbx.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Password");
                su_pssbx.Foreground = new SolidColorBrush(Color.FromRgb(11, 63, 125));
                su_pssbx.Padding = new Thickness(4);
            }
                su_pssbx.Text = "";

        }

        private void su_pssbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (su_pssbx.Text == "")
            {
                su_pssbx.Text = pass_string;
                su_pssbx.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Segoe UI Variable Static Display");
                su_pssbx.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6D1D4"));
                su_pssbx.Padding = new Thickness(0);
            }
            else
                su_pssbx.Padding = new Thickness(4);
        }

        private void su_repssbx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (su_repssbx.Text == pass_string)
            {
                su_repssbx.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Password");
                su_repssbx.Foreground = new SolidColorBrush(Color.FromRgb(11, 63, 125));
                su_repssbx.Padding = new Thickness(4);
            }
                su_repssbx.Text = "";
        }

        private void su_repssbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (su_repssbx.Text == "")
            {
                su_repssbx.Text = pass_string;
                su_repssbx.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Segoe UI Variable Static Display");
                su_repssbx.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC6D1D4"));
                su_repssbx.Padding = new Thickness(0);
            }
            else
                su_repssbx.Padding = new Thickness(4);
        }

        private bool checkIdDuplicate(string username_s, string password_s)
        {
            if (username_s == "admin" || username_s == userID_string)
                return true;

            try
            {
                if (client.SignUp(username_s, password_s))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

            }

            return true;
        }

        private bool checkPasswordMatch(string psswrd, string rep_psswrd)
        {
            return (psswrd == rep_psswrd) && psswrd != pass_string;
        }

        private bool checkStrongPasswordMatch(string psswrd)
        {
            return (psswrd.Length >= 8);
        }


        private void signup_Click(object sender, RoutedEventArgs e)
        {
            List<string> arguments = new List<string>();
            arguments.Add(su_txtbx.Text);
            arguments.Add(su_pssbx.Text);
            arguments.Add(su_repssbx.Text);
            worker_su.RunWorkerAsync(arguments);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Process user input
                signup.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }
    }
}
