using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using MySql.Data.MySqlClient;

namespace Rent_a_car.pages.clients
{
  
    public partial class clientsaddwindow
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        string name;
        string surname;
        string documentno;
        string telefonno;
        int debit;
        public clientsaddwindow()
        {
            InitializeComponent();
        }

        private void Debittxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9+-]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn)
                {
                    Debittxt.IsEnabled = true;
                    
                }
                else
                {
                    Debittxt.IsEnabled = false;
                    Debittxt.Text = "";
                    
                }
            }
        }

        private void clientaddbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(nametxtbox.Text) &&
    !string.IsNullOrWhiteSpace(surnametxt.Text) &&
    !string.IsNullOrWhiteSpace(Documentnotxt.Text) &&
    !string.IsNullOrWhiteSpace(Telnotxt.Text) &&
    !string.IsNullOrWhiteSpace(Debittxt.Text))
            {
                debit = int.Parse(Debittxt.Text);
                name = nametxtbox.Text;
                surname = surnametxt.Text;
                documentno = Documentnotxt.Text;
                telefonno = Telnotxt.Text;

                MySqlConnection connection = null;
                try
                {
                    connection = new MySqlConnection(connectionString);
                    connection.Open();
                    string insertQuery = "INSERT INTO clients (Client_Name, Client_Surname, Client_Document_No, Client_Tel, Client_Debit, Client_Status) VALUES (@name, @surname, @documentno, @telno, @debit, 'aktiv')";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@name", name);
                        insertCmd.Parameters.AddWithValue("@surname", surname);
                        insertCmd.Parameters.AddWithValue("@documentno", documentno);
                        insertCmd.Parameters.AddWithValue("@telno", telefonno);
                        insertCmd.Parameters.AddWithValue("@debit", debit);
                        insertCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filling data: " + ex.Message);
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Iltimos barcha ma'lumot kiriting.", "Xato", MessageBoxButton.OK);
            }
        }
    }
}
