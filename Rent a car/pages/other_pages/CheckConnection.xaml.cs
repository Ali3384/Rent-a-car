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
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace Rent_a_car.pages.other_pages
{
    /// <summary>
    /// Логика взаимодействия для CheckConnection.xaml
    /// </summary>
    public partial class CheckConnection
    {
        private string connectionString = Properties.Settings.Default.ConnectionString;

        public CheckConnection()
        {
            Properties.Settings.Default.connectioncheck = false;
            Properties.Settings.Default.Save();
            InitializeComponent();
            CheckDatabaseConnection();
        }

        private async void CheckDatabaseConnection()
        {
            try
            {
                statusLabel.Content = "Trwa proba polaczenia do serwera...";
                progressRing.IsActive = true;

                bool isConnected = await Task.Run(() => TestConnection());

                if (isConnected)
                {
                    statusLabel.Content = "Polaczenie udane!";
                    this.Close();
                }
                else
                {
                    statusLabel.Content = "Applikacje zostanie zamknieta!";
                    statusLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MessageBox.Show("Nie ma polaczenia z serwerom, Applikacje zostanie zamknieta", "Error", MessageBoxButton.OK);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                statusLabel.Content = "Applikacje zostanie zamknieta!";
                statusLabel.Foreground = new SolidColorBrush(Colors.Red);
                MessageBox.Show("Nie ma polaczenia z serwerom, Applikacje zostanie zamknieta", "Error", MessageBoxButton.OK);
                this.Close();
            }
            finally
            {
                progressRing.IsActive = false;
            }
        }

        private bool TestConnection()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    Properties.Settings.Default.connectioncheck = true;
                    Properties.Settings.Default.Save();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
