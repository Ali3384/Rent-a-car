using ControlzEx.Theming;
using MahApps.Metro.Controls;
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
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;

namespace Rent_a_car
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        double debitstatus;
        public MainWindow()
        {
            InitializeComponent();
            rentframe.Content = new Rent_a_car.pages.rent.rentmainpage();
            carsframe.Content = new Rent_a_car.pages.cars.carsmainpage();
            clientsframe.Content = new Rent_a_car.pages.clients.clientsmainpage();
            paymentsframe.Content = new Rent_a_car.pages.payments.paymentsmainpage();
            UpdateDebitStatus();
        }
        public void ThemeChange(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if(toggleSwitch != null)
            {
                if(toggleSwitch.IsOn)
                {
                    ThemeManager.Current.ChangeTheme(this, "Dark.Steel");
                }
                else
                {
                    ThemeManager.Current.ChangeTheme(this, "Light.Steel");
                }
            }
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
        }
        public void UpdateDebitStatus()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand("SELECT SUM(Client_Debit) FROM clients", connection);
                    object result = command.ExecuteScalar();
                    debitstatus = result != DBNull.Value ? Convert.ToDouble(result) : 0.0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating debit status: " + ex.Message);
            }
            qarzlar.Content = debitstatus.ToString() + " zl";
        }

    }
}
