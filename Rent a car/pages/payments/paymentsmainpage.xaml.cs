using System;
using System.Collections.Generic;
using System.Data;
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
namespace Rent_a_car.pages.payments
{
    /// <summary>
    /// Логика взаимодействия для paymentsmainpage.xaml
    /// </summary>
    public partial class paymentsmainpage : Page
    {
        string periodid;
        string clientid;
        string id;
        int amount;
        string paymentscomboboxselection;
        string paymentsfiltertext;
        DataTable payments = new DataTable();
        string connectionString = Properties.Settings.Default.ConnectionString;
        public paymentsmainpage()
        {
            InitializeComponent();
            fillPaymentsTable();
        }
        private void fillPaymentsTable()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Payment_ID ,Period_ID,Payment_Date,Client_Name,Car_Plate_No,Payment_Amount,Payment_Method, Client_ID FROM payments";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    adapter.Fill(payments);
                    payments.Columns["Payment_ID"].ColumnName = "ID";
                    payments.Columns["Period_ID"].ColumnName = "Okres ID";
                    payments.Columns["Payment_Date"].ColumnName = "Data Opłaty";
                    payments.Columns["Client_Name"].ColumnName = "Imie Klienta";
                    payments.Columns["Car_Plate_No"].ColumnName = "Nr Pojazdu";
                    payments.Columns["Payment_Amount"].ColumnName = "Suma Opłaty";
                    payments.Columns["Payment_Method"].ColumnName = "Typ Opłaty";
                    payments.Columns["Client_ID"].ColumnName = "ID Klienta";
                    paymentsdatagrid.ItemsSource = payments.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while filling data: " + ex.Message);
            }
        }
        private void carsfilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            paymentsfiltertext = paymentsfilter.Text;
            DataView dataView = payments.DefaultView;
            if (string.IsNullOrEmpty(paymentsfiltertext))
            {
                dataView.RowFilter = string.Empty;
            }
            else if (!string.IsNullOrEmpty(paymentscomboboxselection))
            {
                try
                {
                    string escapedFilterText = paymentsfiltertext.Replace("'", "''");
                    string columnName = paymentscomboboxselection.Contains(" ") ? $"[{paymentscomboboxselection}]" : paymentscomboboxselection;
                    dataView.RowFilter = $"{columnName} LIKE '%{escapedFilterText}%'";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filtering data: " + ex.Message);
                }
            }
            paymentsdatagrid.ItemsSource = dataView;
        }
        private void paymentscombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            paymentscomboboxselection = ((ComboBoxItem)paymentscombobox.SelectedItem).Content.ToString();
        }

        private void DeletePayment()
        {
            var selectedItem = paymentsdatagrid.SelectedItem as DataRowView;

            if (selectedItem != null)
            {
                id = selectedItem["ID"].ToString();
            }
            string updateDebitQuery = "DELETE FROM payments WHERE Payment_ID = @id;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand updateDebitCommand = new MySqlCommand(updateDebitQuery, connection);
                updateDebitCommand.Parameters.AddWithValue("@id", id);

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
        private void UpdateClientDebit()
        {
            // Cast the selected item to a DataRowView to access its columns
            var selectedItem = paymentsdatagrid.SelectedItem as DataRowView;

            if (selectedItem != null)
            {
                periodid = selectedItem["Okres ID"].ToString();
                clientid = selectedItem["ID Klienta"].ToString();
            }
            string updateDebitQuery = "UPDATE clients SET Client_Debit = Client_Debit + @Amount WHERE Client_ID = @Client_ID;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand updateDebitCommand = new MySqlCommand(updateDebitQuery, connection);
                updateDebitCommand.Parameters.AddWithValue("@Client_ID", clientid);
                updateDebitCommand.Parameters.AddWithValue("@Amount", amount);

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
            string updateDebitQuery2 = "UPDATE rentperiods SET Payment_Status = 'Niezapłacona' WHERE Period_ID = @periodid;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand updateDebitCommand = new MySqlCommand(updateDebitQuery2, connection);
                updateDebitCommand.Parameters.AddWithValue("@periodid", periodid);

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

        private void deletepayment_Click(object sender, RoutedEventArgs e)
        {
            if(paymentsdatagrid.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Czy chcesz usunąć wybranej opłaty ?",
                     "Czy jesteś pewien ?",
                     MessageBoxButton.YesNo,
                     MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    UpdateClientDebit();
                    DeletePayment();
                }
                        
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            updatePayments();
        }
        public void updatePayments()
        {
            payments.Clear();
            payments.Columns["ID"].ColumnName = "Payment_ID";
            payments.Columns["Okres ID"].ColumnName = "Period_ID";
            payments.Columns["Data Opłaty"].ColumnName = "Payment_Date";
            payments.Columns["Imie Klienta"].ColumnName = "Client_Name";
            payments.Columns["Nr Pojazdu"].ColumnName = "Car_Plate_No";
            payments.Columns["Suma Opłaty"].ColumnName = "Payment_Amount";
            payments.Columns["Typ Opłaty"].ColumnName = "Payment_Method";
            payments.Columns["ID Klienta"].ColumnName = "Client_ID";
            fillPaymentsTable();
            paymentsdatagrid.Items.Refresh();

        }
    }
}
