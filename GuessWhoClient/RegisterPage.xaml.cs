using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class RegisterPage : Page
    {
        private string imagePath = "";

        public RegisterPage()
        {
            InitializeComponent();
        }

        private void BtnUploadProfilePictureClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Archivos de imagen|*.jpg;*.png;";

            if (fileDialog.ShowDialog() == true)
            {
                imagePath = fileDialog.FileName;

                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
                ImageProfile.Source = bitmapImage;
            }
        }

        private void BtnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            string nickname = TbNickname.Text;
            string password = TbPassword.Text;
            string passwordConfirmation = TbRepeatedPassword.Text;
            string email = TbEmail.Text;
            byte[] profileImage = GetProfileImageBytes();

            if(nickname == "" || password == "" || passwordConfirmation == ""
               || email == "")
            {
                MessageBox.Show(
                    "Ingrese información en cada uno de los campos para poder continuar", 
                    "Campos vacíos", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning
                );
                return;
            }

            if(true)
            {
                MessageBox.Show(
                    "Corrija el correo electrónico a un formato válido",
                    "Correo electrónico inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
        }

        private byte[] GetProfileImageBytes()
        {
            if (imagePath == "")
            {
                return null;
            }

            return File.ReadAllBytes(imagePath);
        }
    }
}
