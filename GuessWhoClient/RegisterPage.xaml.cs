﻿using GuessWhoClient.Properties;
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
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);

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
                        resourceManager.GetString("msgbRegisterAlertImageSizeMessage"),
                        resourceManager.GetString("msgbRegisterAlertImageSizeTitle"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbRegisterImageNotFoundMessage"),
                    resourceManager.GetString("msgbRegisterImageNotFoundTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
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
                        resourceManager.GetString("msgbRegisterRegisteredEmailMessage"),
                        resourceManager.GetString("msgbRegisterRegisteredEmailTitle"),
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
                    ServerResponse.GetMessageFromStatusCode(emailValidation.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(emailValidation.StatusCode),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            } else
            {
                if(nicknameValidation.Value != null)
                {
                    MessageBox.Show(
                        resourceManager.GetString("msgbRegisterRegisteredNicknameMessage"),
                        resourceManager.GetString("msgbRegisterRegisteredNicknameTitle"),
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