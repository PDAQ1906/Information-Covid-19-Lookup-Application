using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for DatePickerWindow.xaml
    /// </summary>
    public partial class DatePickerWindow : Window
    {
        private string[] LoadComboBoxMonth()
        {
            string[] MonthArray =
            {
                "January",
                "February",
                "March",
                "April",
                "May",
                "June",
                "July",
                "August",
                "September",
                "October",
                "November",
                "December"
            };
            return MonthArray;
        }

        private string[] LoadComboBoxDate()
        {
            string[] DateArray = new string[31];
            for (int i = 0; i < DateArray.Length; i++)
            {
                DateArray[i] = (i + 1).ToString("00");
            }
            return DateArray;
        }

        private string[] LoadComboBoxYear()
        {
            string[] YearArray = new string[5];
            for (int i = 0; i < YearArray.Length; i++)
            {
                YearArray[i] = (i + 2020).ToString();
            }
            return YearArray;
        }

        public DatePickerWindow(string date_string)
        {
            InitializeComponent();
            Day.ItemsSource = LoadComboBoxDate();
            Month.ItemsSource = LoadComboBoxMonth();
            Year.ItemsSource = LoadComboBoxYear();
            DateTime dt;
            DateTime.TryParseExact(date_string,
                   "dd/MM/yyyy",
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out dt);
            Day.SelectedIndex = (dt.Day - 1);
            Month.SelectedIndex = (dt.Month - 1);
            Year.SelectedIndex = (dt.Year - 2020);
        }



        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //Process user input
                pickDate.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Close();

        }

        private void pickDate_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).Date.Content = Day.SelectedItem.ToString() + "/"
                                                          + (Month.SelectedIndex + 1).ToString("00") + "/" + Year.SelectedItem.ToString();
                    (window as MainWindow).UpdateDataEvent.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    window.Focus();
                }
            }
        }
    }
}
