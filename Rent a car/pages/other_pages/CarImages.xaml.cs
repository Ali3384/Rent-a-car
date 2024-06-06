using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Логика взаимодействия для CarImages.xaml
    /// </summary>
    public partial class CarImages
    {
        public List<BitmapImage> ImageList { get; set; }
        string connectionString = Properties.Settings.Default.ConnectionString;
        string mainimage;
        string images;
        public CarImages()
        {
            InitializeComponent();
            ImageList = new List<BitmapImage>();
            DataContext = this; // Set the DataContext to this window
            GetImages();
        }

        public void GetImages()
        {
            try
            {
                using (MySqlConnection connection2 = new MySqlConnection(connectionString))
                {
                    connection2.Open();
                    MySqlCommand command = new MySqlCommand("SELECT Cars_Image FROM cars WHERE Cars_No = @no", connection2);
                    command.Parameters.AddWithValue("@no", Properties.Settings.Default.CarSelectedCarPlate);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        string imageName = result.ToString();
                        string ftpUrl = $"ftp://admin%2540adigcars.pl@host653587.hostido.net.pl/public_html/images/{Properties.Settings.Default.CarSelectedCarPlate}/{imageName}";

                        BitmapImage bitmap = new BitmapImage();
                        using (MemoryStream stream = new MemoryStream())
                        {
                            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                            request.Method = WebRequestMethods.Ftp.DownloadFile;
                            request.Credentials = new NetworkCredential("admin@adigcars.pl", "M08011998m@");

                            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                            using (Stream responseStream = response.GetResponseStream())
                            {
                                responseStream.CopyTo(stream);
                            }

                            stream.Seek(0, SeekOrigin.Begin);
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                        }

                        mainimagebox.Source = bitmap;
                    }
                    else
                    {
                        MessageBox.Show("No image found for the selected car.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while getting data: " + ex.Message);
            }
        }
    }
}
