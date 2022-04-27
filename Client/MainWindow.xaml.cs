using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Client.ClientTransfer client = new Client.ClientTransfer();
        CountryData countryData = new CountryData();
        public string string_date = "";
        BackgroundWorker worker = new BackgroundWorker();

        public MainWindow(string name)
        {
            InitializeComponent();

            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            accountName.Text = name;
            DateTime dt = DateTime.Now;
            Date.Content = dt.Date.ToString("dd/MM/yyyy");
            Client.CountryData data;
            try
            {
                client.RequestCountryData(searchbx.Text,
                    (short)dt.Day, (short)dt.Month, (short)dt.Year);
                //if (data.Name != "Unknown")
                //{
                //    updateData(ref data);
                //}

            }
            catch (Exception ex)
            {
                MessageWindow mW = new MessageWindow("404: Server stopped!");
                mW.ShowDialog();
                this.Close();
                return;
            }
            worker.RunWorkerAsync(0);
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 2)
            {
                updateData(ref countryData);
            }
            else if (e.ProgressPercentage == -1)
            {
                MessageWindow mW = new MessageWindow("Server turned off!");
                mW.ShowDialog();
                this.Close();
                return;
            }
        }

        bool flag_connect = true;
        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if ((int)e.Argument == -1)
            {
                return;
            }
            while (flag_connect)
            {
                int error_code = client.transferData(ref countryData);
                if (error_code == 2) //server returns country data
                {
                    worker.ReportProgress(error_code);
                }
                else if (error_code == -1)
                {
                    worker.ReportProgress(-1);
                    e.Result = 0;
                    return;
                }

                else if (error_code == 4)
                {
                    e.Result = 0;
                    return;
                }
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            flag_connect = false;
            this.Close();
        }


        private void MainWindowToolBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void searchbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchbx.Text != "")
            {
                plcHolderSearch.Visibility = Visibility.Hidden;
            }
            else
            {
                plcHolderSearch.Visibility = Visibility.Visible;
            }
        }

        private void updateData(ref CountryData data)
        {
            if (data.Name != "Unknown")
            {
                countryName.Text = data.Name;
                totalCases.Text = data.TotalCases.ToString("N0");
                todayCases.Text = "+" + data.CasesToday.ToString("N0");
                totalDeaths.Text = data.TotalDeaths.ToString("N0");
                todayDeaths.Text = "+" + data.DeathsToday.ToString("N0");
                recovered.Text = data.Recovered.ToString("N0");
                uint active = data.TotalCases - data.TotalDeaths - data.Recovered;
                double dpc = 100.0 * (double)data.TotalDeaths / data.TotalCases;
                double rpc = 100.0 * (double)data.Recovered / data.TotalCases;
                activeCases.Text = active.ToString("N0");
                deathsPerCases.Text = dpc.ToString("0.##") + "%";
                recoverPerCases.Text = rpc.ToString("0.##") + "%";
            }
            else
            {
                this.Focus();
                MessageWindow messageWindow = new MessageWindow("Invalid country name or date.");
                messageWindow.ShowDialog();
                searchbx.SelectAll();
            }
        }


        private void searchbx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Storyboard myStoryboard = (Storyboard)searchButton.Resources["SearchAnimation"];
                Storyboard.SetTarget(myStoryboard.Children.ElementAt(0) as DoubleAnimationUsingKeyFrames, searchButton);
                myStoryboard.Begin();

                try
                {
                    DateTime dt;
                    if (!DateTime.TryParseExact((string)Date.Content,
                   "dd/MM/yyyy",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out dt))
                    {
                        this.Focus();
                        MessageWindow messageWindow = new MessageWindow("Not a valid date.");
                        messageWindow.ShowDialog();
                        Date.Content = string_date;
                    }
                    client.RequestCountryData(searchbx.Text,
                            (short)dt.Day, (short)dt.Month, (short)dt.Year);
                }
                catch (Exception ex)
                {
                    MessageWindow mW = new MessageWindow("404: Server stopped!");
                    mW.ShowDialog();
                    this.Close();
                    return;
                }
            }
        }

        private void Date_Click(object sender, RoutedEventArgs e)
        {
            string_date = (string)Date.Content;
            DatePickerWindow dtPk = new DatePickerWindow(string_date);
            dtPk.Show();
        }

        private void UpdateDataEvent_Click(object sender, RoutedEventArgs e)
        {
            //Client.CountryData data;
            try
            {
                DateTime dt;
                if (!DateTime.TryParseExact((string)Date.Content,
                   "dd/MM/yyyy",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out dt))
                {
                    this.Focus();
                    MessageWindow messageWindow = new MessageWindow("Not a valid date.");
                    messageWindow.ShowDialog();
                    Date.Content = string_date;
                    return;
                }
                client.RequestCountryData(searchbx.Text,
                        (short)dt.Day, (short)dt.Month, (short)dt.Year);
                //updateDateData(ref data);

            }
            catch (Exception ex)
            {
                MessageWindow mW = new MessageWindow("404: Server stopped!");
                mW.ShowDialog();
                this.Close();
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                client.closeConnection();

            }
            catch (Exception ex)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow info_ms = new InfoWindow();
            info_ms.Show();
        }
    }
}
