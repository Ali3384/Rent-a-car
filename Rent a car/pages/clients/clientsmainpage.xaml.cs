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
                    string query = "SELECT Client_ID,Client_Name,Client_Surname,Client_Document_No,Client_Tel,Client_Debit FROM clients WHERE Client_Status = 'aktiv'";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    adapter.Fill(clients);
                    clients.Columns["Client_ID"].ColumnName = "Mijoz ID";
                    clients.Columns["Client_Name"].ColumnName = "Ismi";
                    clients.Columns["Client_Surname"].ColumnName = "Familiyasi";
                    clients.Columns["Client_Document_No"].ColumnName = "Hujjat No";
                    clients.Columns["Client_Tel"].ColumnName = "Telefon No";
                    clients.Columns["Client_Debit"].ColumnName = "Mijoz qarzi";
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
                int debit = int.Parse(rowView["Mijoz Qarzi"].ToString());
                
                    if (debit < 0)
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
            clients.Columns["Mijoz ID"].ColumnName = "Client_ID";
            clients.Columns["Ismi"].ColumnName = "Client_Name";
            clients.Columns["Familiyasi"].ColumnName = "Client_Surname";
            clients.Columns["Hujjat No"].ColumnName = "Client_Document_No";
            clients.Columns["Telefon No"].ColumnName = "Client_Tel";
            clients.Columns["Mijoz qarzi"].ColumnName = "Client_Debit";
            fillClientsTable();
            clientsdatagrid.Items.Refresh();
        }
    }
}
