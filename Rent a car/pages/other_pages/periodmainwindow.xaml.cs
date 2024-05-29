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
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace Rent_a_car.pages.other_pages
{
    /// <summary>
    /// Логика взаимодействия для periodmainwindow.xaml
    /// </summary>
    public partial class periodmainwindow
    {
        string connectionString = Properties.Settings.Default.ConnectionString;
        DataTable periods = new DataTable();
        string query = "";
        public periodmainwindow()
        {
            InitializeComponent();
            fillPeriodsTable();
        }
        private void fillPeriodsTable()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    query = Properties.Settings.Default.FillQuery;
                    
                    MySqlCommand command = new MySqlCommand(query, connection);
                    if (Properties.Settings.Default.IsRentings)
                    {
                        command.Parameters.AddWithValue("@clientname", Properties.Settings.Default.RentClientName);
                        command.Parameters.AddWithValue("@plateno", Properties.Settings.Default.RentCarPlate);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@clientid", Properties.Settings.Default.RentClientID);
                    }
                    
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    adapter.Fill(periods);
                    periods.Columns["Period_ID"].ColumnName = "ID";
                    periods.Columns["Client_Name"].ColumnName = "Imie klienta";
                    periods.Columns["Car_Plate_No"].ColumnName = "Nr Pojazdu";
                    periods.Columns["Period_From"].ColumnName = "Od";
                    periods.Columns["Period_Until"].ColumnName = "Do";
                    periods.Columns["Period_Cost"].ColumnName = "Koszt najmu";
                    periods.Columns["Payment_Status"].ColumnName = "Opłata";
                    periods.Columns["Payment_Amount"].ColumnName = "Zapłacona (zl)";
                    periods.Columns["Client_ID"].ColumnName = "Klient ID";
                    periodsdatagrid.ItemsSource = periods.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while filling data: " + ex.Message);
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            Window current = Window.GetWindow(this);
            current.Close();
        }

        private void periodsdatagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataRowView rowView = e.Row.Item as DataRowView;
            if (rowView != null)
            {
                string paymentstatus = rowView["Opłata"].ToString();
                if(paymentstatus == "Niezapłacona")
                {
                    e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 127, 127));
                }
            }
        }

        private void addpayment_Click(object sender, RoutedEventArgs e)
        {
            if (periodsdatagrid.SelectedItem != null)
            {

                // Cast the selected item to a DataRowView to access its columns
                var selectedItem = periodsdatagrid.SelectedItem as DataRowView;

                if (selectedItem != null)
                {
                    if (selectedItem["Opłata"].ToString() == "Niezapłacona")
                    {
                        Properties.Settings.Default.PeriodCarPlate = selectedItem["Nr Pojazdu"].ToString();
                        Properties.Settings.Default.PeriodID = selectedItem["ID"].ToString();
                        Properties.Settings.Default.PeriodClientName = selectedItem["Imie klienta"].ToString();
                        Properties.Settings.Default.PeriodClientID = selectedItem["Klient ID"].ToString();
                        Properties.Settings.Default.Save();
                        addpaymentwindow addpaymentwindow = new addpaymentwindow();
                        addpaymentwindow.ShowDialog();


                        UpdatePeriods();
                    }
                    else
                    {
                        MessageBox.Show("Wybrany okres jest opłacony !!!", "Błąd");
                    }
                    
                }

            }
        }
        public void UpdatePeriods()
        {
            periods.Clear();
            periods.Columns["ID"].ColumnName = "Period_ID";
            periods.Columns["Imie klienta"].ColumnName = "Client_Name";
            periods.Columns["Nr Pojazdu"].ColumnName = "Car_Plate_No";
            periods.Columns["Od"].ColumnName = "Period_From";
            periods.Columns["Do"].ColumnName = "Period_Until";
            periods.Columns["Koszt najmu"].ColumnName = "Period_Cost";
            periods.Columns["Opłata"].ColumnName = "Payment_Status";
            periods.Columns["Zapłacona (zl)"].ColumnName = "Payment_Amount";
            periods.Columns["Klient ID"].ColumnName = "Client_ID";
            fillPeriodsTable();
            periodsdatagrid.Items.Refresh();


        }

    }
}
