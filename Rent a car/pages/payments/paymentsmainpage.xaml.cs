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
                    payments.Columns["Period_ID"].ColumnName = "Period ID";
                    payments.Columns["Payment_Date"].ColumnName = "To'lov Sanasi";
                    payments.Columns["Client_Name"].ColumnName = "Mijoz Ismi";
                    payments.Columns["Car_Plate_No"].ColumnName = "Avtomobil raqami";
                    payments.Columns["Payment_Amount"].ColumnName = "To'lov miqdori";
                    payments.Columns["Payment_Method"].ColumnName = "To'lov turi";
                    payments.Columns["Client_ID"].ColumnName = "Mijoz ID";
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
                periodid = selectedItem["Period ID"].ToString();
                clientid = selectedItem["Mijoz ID"].ToString();
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
            string updateDebitQuery2 = "UPDATE rentperiods SET Payment_Status = 'Tolanmagan' WHERE Period_ID = @periodid;";

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
                if (MessageBox.Show("To'lovni o'chirib tashlamoqchimisiz ?",
                     "O'chirish",
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
            payments.Columns["Period ID"].ColumnName = "Period_ID";
            payments.Columns["To'lov Sanasi"].ColumnName = "Payment_Date";
            payments.Columns["Mijoz Ismi"].ColumnName = "Client_Name";
            payments.Columns["Avtomobil raqami"].ColumnName = "Car_Plate_No";
            payments.Columns["To'lov miqdori"].ColumnName = "Payment_Amount";
            payments.Columns["To'lov turi"].ColumnName = "Payment_Method";
            payments.Columns["Mijoz ID"].ColumnName = "Client_ID";
            fillPaymentsTable();
            paymentsdatagrid.Items.Refresh();

        }
    }
}
