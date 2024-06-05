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

namespace Rent_a_car.pages.rent
{
    /// <summary>
    /// Логика взаимодействия для rentaddpage1.xaml
    /// </summary>
    public partial class rentaddpage1 : Page
    {
        string clientscomboboxselection;
        string clientsfiltertext;
        DataTable clients = new DataTable();
        string connectionString = Properties.Settings.Default.ConnectionString;
        public rentaddpage1()
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
                    clients.Columns["Client_Document_No"].ColumnName = "Dowód No";
                    clients.Columns["Client_Tel"].ColumnName = "Telefon No";
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
            }
            else if (!string.IsNullOrEmpty(clientscomboboxselection))
            {
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

        private void nextbtn_Click(object sender, RoutedEventArgs e)
        {
            if(clientsdatagrid.SelectedItems.Count > 0)
            {
                foreach(DataRowView row in  clientsdatagrid.SelectedItems)
                {
                    System.Data.DataRow myRow = row.Row;
                    Properties.Settings.Default.SelectedClientName = myRow["Imie"].ToString();
                    Properties.Settings.Default.SelectedClientSurname = myRow["Nazwisko"].ToString();
                    Properties.Settings.Default.SelectedClientID = myRow["ID"].ToString();
                    Properties.Settings.Default.Save();
                }
                NavigationService.Navigate(new rentaddpage2());
            }
        }

        private void clientsdatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void clientsdatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (clientsdatagrid.SelectedItems.Count > 0)
            {
                foreach (DataRowView row in clientsdatagrid.SelectedItems)
                {
                    System.Data.DataRow myRow = row.Row;
                    Properties.Settings.Default.SelectedClientName = myRow["Imie"].ToString();
                    Properties.Settings.Default.SelectedClientSurname = myRow["Nazwisko"].ToString();
                    Properties.Settings.Default.SelectedClientID = myRow["ID"].ToString();
                    Properties.Settings.Default.Save();
                }
                NavigationService.Navigate(new rentaddpage2());
            }
        }
    }
}
