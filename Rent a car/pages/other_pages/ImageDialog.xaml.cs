using System.Windows;
using System.Windows.Media.Imaging;

namespace Rent_a_car.pages.other_pages
{
    public partial class ImageDialog
    {
        public ImageDialog()
        {
            InitializeComponent();
        }

        public void SetImageSource(BitmapImage image)
        {
            imageControl.Source = image;
        }
    }
}