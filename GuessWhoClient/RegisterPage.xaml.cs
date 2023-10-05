using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BtnUploadProfilePictureClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Archivos de imagen|*.jpg;*.png;*.bmp|Todos los archivos|*.*";

            if (fileDialog.ShowDialog() == true)
            {
                string imagePath = fileDialog.FileName;

                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
                ImageProfile.Source = bitmapImage;
            }
        }

        private void BtnCreateAccountClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
