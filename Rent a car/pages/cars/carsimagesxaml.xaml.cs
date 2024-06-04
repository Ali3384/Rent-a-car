using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Rent_a_car.pages.cars
{
    public class ImageInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для carsimagesxaml.xaml
    /// </summary>
    public partial class carsimagesxaml : Window
    {
        public List<ImageInfo> Images { get; set; }

        public carsimagesxaml()
        {
            InitializeComponent();
            Images = new List<ImageInfo>();
        }

        private void imageadd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.BMP;*.JPG;*.JPEG;*.PNG;*.TIFF;*.TIF;*.ICO;*.HEIC;*.WEBP",
                Multiselect = true,
                Title = "Please choose image files"
            };

            bool? success = fileDialog.ShowDialog();
            if (success == true)
            {
                Images.Clear();
                foreach (var filePath in fileDialog.FileNames)
                {
                    Images.Add(new ImageInfo
                    {
                        FileName = System.IO.Path.GetFileName(filePath),
                        FilePath = filePath
                    });
                }

                RefreshDataGrid();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is ImageInfo imageInfo)
            {
                Images.Remove(imageInfo);
                RefreshDataGrid();
            }
        }

        private void RefreshDataGrid()
        {
            listofimages.ItemsSource = null;
            listofimages.ItemsSource = Images;
        }
    }
}
