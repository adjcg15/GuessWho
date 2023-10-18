using GuessWhoClient.Properties;
using GuessWhoClient.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.Resources;
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
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
            string fullName = TbName.Text;
            string nickname = TbNickname.Text;
            string password = TbPassword.Text;
            string passwordConfirmation = TbRepeatedPassword.Text;
            string email = TbEmail.Text;
            byte[] profileImage = ImageTransformator.GetImageBytesFromImagePath(imagePath);

            if (nickname == "" || password == "" || passwordConfirmation == ""
               || email == "" || fullName == "")
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterEmptyFieldsMessage"),
                    resourceManager.GetString("msgbRegisterEmptyFieldsTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsValidNickname(nickname))
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterInvalidNicknameMessage"),
                    resourceManager.GetString("msgbRegisterInvalidNicknameTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsValidEmail(email))
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterInvalidEmailMessage"),
                    resourceManager.GetString("msgbRegisterInvalidEmailTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsSecurePassword(password))
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterInsecurePasswordMessage"),
                    resourceManager.GetString("msgbRegisterInsecurePasswordTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (password != passwordConfirmation)
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterMismatchPasswordMessage"),
                    resourceManager.GetString("msgbRegisterMismatchPasswordTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            GameServices.AuthenticationServiceClient authenticationServiceClient = new GameServices.AuthenticationServiceClient();
            bool userEmailAlreadyRegistered = authenticationServiceClient.VerifyUserRegisteredByEmail(email);
            if (userEmailAlreadyRegistered)
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterRegisteredEmailMessage"),
                    resourceManager.GetString("msgbRegisterRegisteredEmailTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            bool userNicknameAlreadyRegistered = authenticationServiceClient.VerifyUserRegisteredByNickName(nickname);
            if (userNicknameAlreadyRegistered)
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterRegisteredNicknameMessage"),
                    resourceManager.GetString("msgbRegisterRegisteredNicknameTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            ConfirmAccountPage confirmAccountPage = new ConfirmAccountPage(nickname, email, password, fullName, profileImage);
            this.NavigationService.Navigate(confirmAccountPage);
        }
    }
}
