using GuessWhoClient.Utils;
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
            string fullname = TbName.Text;
            string nickname = TbNickname.Text;
            string password = TbPassword.Text;
            string passwordConfirmation = TbRepeatedPassword.Text;
            string email = TbEmail.Text;
            byte[] profileImage = GetProfileImageBytes();

            if (nickname == "" || password == "" || passwordConfirmation == ""
               || email == "")
            {
                MessageBox.Show(
                    "ingrese información en cada uno de los campos para poder continuar",
                    "campos vacíos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsValidEmail(email))
            {
                MessageBox.Show(
                    "corrija el correo electrónico a un formato válido",
                    "correo electrónico inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (password != passwordConfirmation)
            {
                MessageBox.Show(
                    "las contraseñas ingresadas no coinciden, modifique los campos para que lo hagan",
                    "contraseñas no coincidentes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            GameServices.UserServiceClient userServiceClient = new GameServices.UserServiceClient();
            GameServices.Profile profile = new GameServices.Profile
            {
                Email = email,
                FullName = fullname,
                Password = Authentication.HashPassword(password),
                NickName = nickname,
                Avatar = profileImage
            };

            bool result = userServiceClient.RegisterUser(profile);
            if (result)
            {
                MessageBox.Show("Se ha registrado el usuario correctamente");
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
