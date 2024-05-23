using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using MySql.Data.MySqlClient;
namespace Rent_a_car.pages.rent
{
    /// <summary>
    /// Логика взаимодействия для rentaddpage4window.xaml
    /// </summary>
    public partial class rentaddpage4window 
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        
        public rentaddpage4window()
        {
            InitializeComponent();
        }

        private void Costtxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void rentaddbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(costtxt.Text) && datefrompicker.SelectedDate.HasValue)
            {

                MySqlConnection connection = null;
                try
                {
                    connection = new MySqlConnection(connectionString);
                    connection.Open();
                    string insertQuery = "INSERT INTO rentings (Rent_Car_Plate_No, Rent_Client_Name, Rent_Client_Surname, Rent_Cost, Rent_From, Rent_Status) VALUES (@plate, @name, @surname, @cost, @from, 'Aktiv')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@plate", Properties.Settings.Default.SelectedCarPlate);
                        insertCmd.Parameters.AddWithValue("@name", Properties.Settings.Default.SelectedClientName);
                        insertCmd.Parameters.AddWithValue("@surname", Properties.Settings.Default.SelectedClientSurname);                        
                        insertCmd.Parameters.AddWithValue("@cost", int.Parse(costtxt.Text));
                        insertCmd.Parameters.AddWithValue("@from", datefrompicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                        insertCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filling data into rentings: " + ex.Message);
                }
                
                try
                {
                    string periodUntil = datefrompicker.SelectedDate.Value.AddDays(7).ToString("yyyy-MM-dd");
                    connection = new MySqlConnection(connectionString);
                    connection.Open();
                    string insertQuery = "INSERT INTO rentperiods (Car_Plate_No, Client_Name, Payment_Status, Period_Cost, Period_From, Period_Until, Period_Status, Client_ID) VALUES (@plate, @name, 'Tolanmagan', @cost, @from, @until, 'aktiv', @clientid)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@plate", Properties.Settings.Default.SelectedCarPlate);
                        insertCmd.Parameters.AddWithValue("@name", Properties.Settings.Default.SelectedClientName);
                        insertCmd.Parameters.AddWithValue("@cost", int.Parse(costtxt.Text));
                        insertCmd.Parameters.AddWithValue("@from", datefrompicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                        insertCmd.Parameters.AddWithValue("@until", periodUntil);
                        insertCmd.Parameters.AddWithValue("@clientid", Properties.Settings.Default.SelectedClientID);
                        insertCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filling data into rentperiods: " + ex.Message);
                }
                inactiveCar();
                this.Close();
            }
            else
            {
                MessageBox.Show("Iltimos barcha ma'lumot kiriting.", "Hato", MessageBoxButton.OK);
            }
        }
        private void inactiveCar()
        {
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                string insertQuery = "UPDATE cars SET Cars_Status = 'Arendada' WHERE Cars_No = @plate";
                using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@plate", Properties.Settings.Default.SelectedCarPlate);
                    insertCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating data: " + ex.Message);
            }
        }

    }
}
