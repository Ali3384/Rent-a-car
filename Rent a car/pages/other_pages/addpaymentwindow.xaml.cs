using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
namespace Rent_a_car.pages.other_pages
{
    /// <summary>
    /// Логика взаимодействия для addpaymentwindow.xaml
    /// </summary>
    public partial class addpaymentwindow
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        public addpaymentwindow()
        {
            InitializeComponent();
            paymentdate.SelectedDate = DateTime.Now;
        }

        private void paymentaddbtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(amount.Text) && paymentdate.SelectedDate.HasValue && method.SelectedItem != null)
            {

                MySqlConnection connection = null;
                try
                {
                    connection = new MySqlConnection(connectionString);
                    connection.Open();
                    string insertQuery = "INSERT INTO payments (Period_ID, Payment_Date, Client_Name, Car_Plate_No, Payment_Amount, Payment_Method, Client_ID) VALUES (@id, @date, @name, @plateno, @amount, @method,@clientid)";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@id", int.Parse(Properties.Settings.Default.PeriodID));
                        insertCmd.Parameters.AddWithValue("@date", paymentdate.SelectedDate.Value.ToString("yyyy-MM-dd"));
                        insertCmd.Parameters.AddWithValue("@name", Properties.Settings.Default.PeriodClientName);
                        insertCmd.Parameters.AddWithValue("@plateno", Properties.Settings.Default.PeriodCarPlate);
                        insertCmd.Parameters.AddWithValue("@amount", int.Parse(amount.Text));
                        insertCmd.Parameters.AddWithValue("@method", ((ComboBoxItem)method.SelectedItem).Content.ToString());
                        insertCmd.Parameters.AddWithValue("@clientid", Properties.Settings.Default.PeriodClientID);
                        insertCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filling data into rentings: " + ex.Message);
                }

                try
                {
                    connection = new MySqlConnection(connectionString);
                    connection.Open();
                    
                    string insertQuery = "UPDATE rentperiods SET Payment_Status = 'Tolangan' WHERE Period_ID = @id";
                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@id", Properties.Settings.Default.PeriodID);
                        insertCmd.ExecuteNonQuery();
                    }
                    UpdateClientDebit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while updating data: " + ex.Message);
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Iltimos barcha ma'lumot kiriting.", "Xato", MessageBoxButton.OK);
            }
        }
        private void Costtxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void UpdateClientDebit()
        {
            string updateDebitQuery = "UPDATE clients SET Client_Debit = Client_Debit - @Amount WHERE Client_ID = @Client_ID;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand updateDebitCommand = new MySqlCommand(updateDebitQuery, connection);
                updateDebitCommand.Parameters.AddWithValue("@Client_ID", Properties.Settings.Default.PeriodClientID);
                updateDebitCommand.Parameters.AddWithValue("@Amount", int.Parse(amount.Text));

                try
                {
                    connection.Open();
                    updateDebitCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}
