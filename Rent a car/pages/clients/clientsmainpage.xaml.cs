using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using Rent_a_car.pages.other_pages;
namespace Rent_a_car.pages.clients

{
    /// <summary>
    /// Логика взаимодействия для clientsmainpage.xaml
    /// </summary>
    public partial class clientsmainpage : Page
    {
        string clientscomboboxselection;
        string clientsfiltertext;
        DataTable clients = new DataTable();
        string connectionString = Properties.Settings.Default.ConnectionString;
        public clientsmainpage()
        {
            InitializeComponent();
            fillClientsTable();
        }
        private void fillClientsTable()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Client_ID,Client_Name,Client_Surname,Client_Document_No,Client_Tel,Client_Debit FROM clients WHERE Client_Status = 'aktywny'";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    adapter.Fill(clients);
                    clients.Columns["Client_ID"].ColumnName = "ID";
                    clients.Columns["Client_Name"].ColumnName = "Imie";
                    clients.Columns["Client_Surname"].ColumnName = "Nazwisko";
                    clients.Columns["Client_Document_No"].ColumnName = "Dowód Nr";
                    clients.Columns["Client_Tel"].ColumnName = "Telefon Nr";
                    clients.Columns["Client_Debit"].ColumnName = "Dług klienta";
                    clientsdatagrid.ItemsSource = clients.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while filling data: " + ex.Message);
            }
        }

        private void clientsfilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            clientsfiltertext = clientsfilter.Text;
            DataView dataView = clients.DefaultView;
            if (string.IsNullOrEmpty(clientsfiltertext))
            {
                dataView.RowFilter = string.Empty;
            }else if (!string.IsNullOrEmpty(clientscomboboxselection)){
                try
                {
                    string escapedFilterText = clientsfiltertext.Replace("'", "''");
                    string columnName = clientscomboboxselection.Contains(" ") ? $"[{clientscomboboxselection}]" : clientscomboboxselection;
                    dataView.RowFilter = $"{columnName} LIKE '%{escapedFilterText}%'";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while filtering data: " + ex.Message);
                }
            }
            clientsdatagrid.ItemsSource = dataView;
        }

        private void clientscombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clientscomboboxselection = ((ComboBoxItem)clientscombobox.SelectedItem).Content.ToString();
        }

        private void clientsdatagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataRowView rowView = e.Row.Item as DataRowView;
            if (rowView != null)
            {
                int debit = int.Parse(rowView["Dług klienta"].ToString());
                
                    if (debit > 400)
                    {
                        e.Row.Background = new SolidColorBrush(Colors.Red);
                    }
                
            }
        }

        private void addclient_Click(object sender, RoutedEventArgs e)
        {
            clientsaddwindow clientsaddwindow = new clientsaddwindow();
            clientsaddwindow.ShowDialog();
            updateClient();
        }
        public void updateClient()
        {
            clients.Clear();
            clients.Columns["ID"].ColumnName = "Client_ID";
            clients.Columns["Imie"].ColumnName = "Client_Name";
            clients.Columns["Nazwisko"].ColumnName = "Client_Surname";
            clients.Columns["Dowód Nr"].ColumnName = "Client_Document_No";
            clients.Columns["Telefon Nr"].ColumnName = "Client_Tel";
            clients.Columns["Dług klienta"].ColumnName = "Client_Debit";
            fillClientsTable();
            clientsdatagrid.Items.Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            updateClient();
        }

        private void deleteclient_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy chcesz usunąć wybranego klienta ?",
                    "Czy jesteś pewien ?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (clientsdatagrid.SelectedItem != null)
                {
                    // Cast the selected item to a DataRowView to access its columns
                    var selectedItem = clientsdatagrid.SelectedItem as DataRowView;

                    if (selectedItem != null)
                    {
                        // Get the value of the "Numer Vin" column
                        string numerVin = selectedItem["ID"].ToString();
                        try
                        {
                            MySqlConnection connection = new MySqlConnection(connectionString);
                            connection.Open();

                            string updateQuery = "UPDATE clients SET Client_Status = 'nie aktywny' WHERE Client_ID = @id";
                            using (MySqlCommand updateCmd = new MySqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@id", numerVin);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error while updating status of client");
                        }
                    }
                    updateClient();
                }
            }
        }

        private void clientsdatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (clientsdatagrid.SelectedItem != null)
            {

                // Cast the selected item to a DataRowView to access its columns
                var selectedItem = clientsdatagrid.SelectedItem as DataRowView;

                if (selectedItem != null)
                {
                    Properties.Settings.Default.RentClientID = selectedItem["ID"].ToString();
                    Properties.Settings.Default.IsRentings = false;
                    Properties.Settings.Default.FillQuery = "SELECT Period_ID,Client_Name,Car_Plate_No,Period_From,Period_Until,Period_Cost,Payment_Status, Payment_Amount, Client_ID FROM rentperiods WHERE Payment_Status = 'Niezapłacona' AND Client_ID = @clientid";

                    Properties.Settings.Default.Save();
                    periodmainwindow periodmainwindow = new periodmainwindow();
                    periodmainwindow.ShowDialog();

                    updateClient();
                }

            }
        }
    }
}
