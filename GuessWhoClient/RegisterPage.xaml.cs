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
            string fullName = TbName.Text;
            string nickname = TbNickname.Text;
            string password = TbPassword.Text;
            string passwordConfirmation = TbRepeatedPassword.Text;
            string email = TbEmail.Text;
            byte[] profileImage = GetProfileImageBytes();

            if (nickname == "" || password == "" || passwordConfirmation == ""
               || email == "" || fullName == "")
            {
                MessageBox.Show(
                    "Ingrese información en cada uno de los campos para poder continuar",
                    "Campos vacíos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsValidNickname(nickname))
            {
                MessageBox.Show(
                    "El nombre de usuario solo debe conetener letras, números y guiones bajos",
                    "Nombre de usuario inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsValidEmail(email))
            {
                MessageBox.Show(
                    "Corrija el correo electrónico a un formato válido",
                    "Correo electrónico inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsSecurePassword(password))
            {
                MessageBox.Show(
                    "La contraseña debe tener mínimo 8 caracteres y, al menos, una letra minúscula, una letra mayúscula, un número y un caracter especial (¡, @, -, $, #, %, _, etc.).",
                    "Contraseñas insegura",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (password != passwordConfirmation)
            {
                MessageBox.Show(
                    "Las contraseñas ingresadas no coinciden, modifique los campos para que lo hagan",
                    "Contraseñas no coincidentes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            GameServices.UserServiceClient userServiceClient = new GameServices.UserServiceClient();
            bool userEmailAlreadyRegistered = userServiceClient.VerifyUserRegisteredByEmail(email);
            if (userEmailAlreadyRegistered)
            {
                MessageBox.Show(
                    "Ya existe una cuenta registrada con ese correo electrónico, intente con otro",
                    "Correo electrónico registrado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            bool userNicknameAlreadyRegistered = userServiceClient.VerifyUserRegisteredByNickName(nickname);
            if (userNicknameAlreadyRegistered)
            {
                MessageBox.Show(
                    "Ya existe otro jugador utilizando el nombre de usuario ingresado, intente con otro",
                    "Nombre de usuario registrado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            ConfirmAccountPage confirmAccountPage = new ConfirmAccountPage(nickname, email, password, fullName, profileImage);
            this.NavigationService.Navigate(confirmAccountPage);
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
