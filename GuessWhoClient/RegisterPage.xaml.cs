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
                UpdateInterfaceImageElement();
            }
        }

        private void UpdateInterfaceImageElement()
        {
            try
            {
                FileInfo fileInfo = new FileInfo(imagePath);
                long fileSizeInBytes = fileInfo.Length;
                long fileSizeInKilobytes = fileSizeInBytes / 1024;

                if (fileSizeInKilobytes <= 20)
                {
                    BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
                    ImageProfile.Source = bitmapImage;
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.msgbRegisterAlertImageSizeMessage,
                        Properties.Resources.msgbRegisterAlertImageSizeTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterImageNotFoundMessage,
                    Properties.Resources.msgbRegisterImageNotFoundTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void BtnCreateAccountClick(object sender, RoutedEventArgs e)
        {
            string fullName = TbName.Text;
            string nickname = TbNickname.Text;
            string password = PbPassword.Password;
            string passwordConfirmation = PbRepeatedPassword.Password;
            string email = TbEmail.Text;
            byte[] profileImage = ImageTransformator.GetImageBytesFromImagePath(imagePath);

            if (nickname == "" || password == "" || passwordConfirmation == ""
               || email == "" || fullName == "")
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterEmptyFieldsMessage,
                    Properties.Resources.msgbRegisterEmptyFieldsTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsValidNickname(nickname))
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterInvalidNicknameMessage,
                    Properties.Resources.msgbRegisterInvalidNicknameTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsValidEmail(email))
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterInvalidEmailMessage,
                    Properties.Resources.msgbRegisterInvalidEmailTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Authentication.IsSecurePassword(password))
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterInsecurePasswordMessage,
                    Properties.Resources.msgbRegisterInsecurePasswordTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (password != passwordConfirmation)
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterMismatchPasswordMessage,
                    Properties.Resources.msgbRegisterMismatchPasswordTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var authenticationServiceClient = new GameServices.AuthenticationServiceClient();

            var emailValidation = authenticationServiceClient.VerifyUserRegisteredByEmail(email);
            if (emailValidation.StatusCode != GameServices.ResponseStatus.OK)
            {
                MessageBox.Show(
                    ServerResponse.GetMessageFromStatusCode(emailValidation.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(emailValidation.StatusCode),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            } 
            else
            {
                if(emailValidation.Value != null)
                {
                    MessageBox.Show(
                        Properties.Resources.msgbRegisterRegisteredEmailMessage,
                        Properties.Resources.msgbRegisterRegisteredEmailTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
            }

            var nicknameValidation = authenticationServiceClient.VerifyUserRegisteredByNickName(nickname);
            if (nicknameValidation.StatusCode != GameServices.ResponseStatus.OK)
            {
                MessageBox.Show(
                    ServerResponse.GetMessageFromStatusCode(nicknameValidation.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(nicknameValidation.StatusCode),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            } else
            {
                if(nicknameValidation.Value != null)
                {
                    MessageBox.Show(
                        Properties.Resources.msgbRegisterRegisteredNicknameMessage,
                        Properties.Resources.msgbRegisterRegisteredNicknameTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }
            }

            ConfirmAccountPage confirmAccountPage = new ConfirmAccountPage(nickname, email, password, fullName, profileImage);
            this.NavigationService.Navigate(confirmAccountPage);
        }
    }
}