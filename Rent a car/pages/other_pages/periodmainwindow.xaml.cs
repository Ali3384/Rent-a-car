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
                    string query = "SELECT Period_ID,Client_Name,Car_Plate_No,Period_From,Period_Until,Period_Cost,Payment_Status, Client_ID FROM rentperiods WHERE Rent_Status = 'aktiv' AND Client_Name = @clientname AND Car_Plate_No = @plateno";
                    
                    MySqlCommand command = new MySqlCommand(query, connection);
                   
                    command.Parameters.AddWithValue("@clientname", Properties.Settings.Default.RentClientName);
                    command.Parameters.AddWithValue("@plateno", Properties.Settings.Default.RentCarPlate);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    adapter.Fill(periods);
                    periods.Columns["Period_ID"].ColumnName = "ID";
                    periods.Columns["Client_Name"].ColumnName = "Mijoz Ismi";
                    periods.Columns["Car_Plate_No"].ColumnName = "Avto raqami";
                    periods.Columns["Period_From"].ColumnName = "Qachondan";
                    periods.Columns["Period_Until"].ColumnName = "Qachongacha";
                    periods.Columns["Period_Cost"].ColumnName = "Arenda narxi";
                    periods.Columns["Payment_Status"].ColumnName = "To'lov";
                    periods.Columns["Client_ID"].ColumnName = "Mijoz ID";
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
                string paymentstatus = rowView["To'lov"].ToString();
                if(paymentstatus == "Tolanmagan")
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
                    if (selectedItem["To'lov"].ToString() == "Tolanmagan")
                    {
                        Properties.Settings.Default.PeriodCarPlate = selectedItem["Avto raqami"].ToString();
                        Properties.Settings.Default.PeriodID = selectedItem["ID"].ToString();
                        Properties.Settings.Default.PeriodClientName = selectedItem["Mijoz Ismi"].ToString();
                        Properties.Settings.Default.PeriodClientID = selectedItem["Mijoz ID"].ToString();
                        Properties.Settings.Default.Save();
                        addpaymentwindow addpaymentwindow = new addpaymentwindow();
                        addpaymentwindow.ShowDialog();


                        UpdatePeriods();
                    }
                    else
                    {
                        MessageBox.Show("Tanlangan ijara davri to'langan !!!", "Xato");
                    }
                    
                }

            }
        }
        public void UpdatePeriods()
        {
            periods.Clear();
            periods.Columns["ID"].ColumnName = "Period_ID";
            periods.Columns["Mijoz Ismi"].ColumnName = "Client_Name";
            periods.Columns["Avto raqami"].ColumnName = "Car_Plate_No";
            periods.Columns["Qachondan"].ColumnName = "Period_From";
            periods.Columns["Qachongacha"].ColumnName = "Period_Until";
            periods.Columns["Arenda narxi"].ColumnName = "Period_Cost";
            periods.Columns["To'lov"].ColumnName = "Payment_Status";
            periods.Columns["Mijoz ID"].ColumnName = "Client_ID";
            fillPeriodsTable();
            periodsdatagrid.Items.Refresh();


        }

    }
}
