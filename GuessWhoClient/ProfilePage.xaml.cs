using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class ProfilePage : Page
    {
        private const string DEFAULT_PROFILE_PICTURE_ROUTE = "pack://application:,,,/Resources/user-icon.png";
        private string imagePath = "";

        public ProfilePage()
        {
            InitializeComponent();
            StablishUIValuesFromProfileInfo();
        }

        private void StablishUIValuesFromProfileInfo()
        {
            LbEmail.Content = DataStore.Profile.Email;
            TbFullName.Text = DataStore.Profile.FullName;
            TbNickname.Text = DataStore.Profile.NickName;

            BitmapImage bitmapAvatar = ImageTransformator.GetBitmapImageFromByteArray(DataStore.Profile.Avatar);
            if (bitmapAvatar != null)
            {
                ImgAvatar.Source = bitmapAvatar;
            }
            else
            {
                Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
                BitmapImage defaultImage = new BitmapImage(uri);
                ImgAvatar.Source = defaultImage;
            }
        }

        private void ImgEditNicknameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowEditNicknameFieldOptions();
            TbNickname.IsEnabled = true;
            TbNickname.Focus();
        }

        private void ImgCloseEditNicknameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HideEditNicknameFieldOptions();
            TbNickname.IsEnabled = false;
            TbNickname.Text = DataStore.Profile.NickName;
        }

        private void ShowEditNicknameFieldOptions()
        {
            GridEditNicknameOptions.Visibility = Visibility.Visible;
            ImgEditNicknameIcon.Visibility = Visibility.Collapsed;
            TbNickname.Width = 241;
        }

        private void HideEditNicknameFieldOptions()
        {
            GridEditNicknameOptions.Visibility = Visibility.Collapsed;
            ImgEditNicknameIcon.Visibility = Visibility.Visible;
            TbNickname.Width = 274;
        }

        private void BtnUpdateImageClick(object sender, RoutedEventArgs e)
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
                    UpdateNewProfileImage(ImageTransformator.GetImageBytesFromImagePath(imagePath));
                    ImgAvatar.Source = bitmapImage;
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
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }
        }

        private void BtnChangePasswordClick(object sender, RoutedEventArgs e)
        {
            string newPassword = PbPassword.Password.Trim();
            string newPasswordConfirmation = PbPasswordConfirmation.Password.Trim();

            ValidateNewPassword(newPassword, newPasswordConfirmation);

            PbPassword.Password = "";
            PbPasswordConfirmation.Password = "";
        }

        private void ValidateNewPassword(string newPassword, string newPasswordConfirmation)
        {
            if (newPassword == "" || newPasswordConfirmation == "")
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterEmptyFieldsMessage,
                    Properties.Resources.msgbRegisterEmptyFieldsTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else if (Authentication.HashPassword(newPassword) == DataStore.Profile.Password)
            {
                MessageBox.Show(
                    Properties.Resources.txtSamePasswordChangeErrorMessage,
                    Properties.Resources.txtSamePasswordChangeErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else if (!Authentication.IsSecurePassword(newPassword))
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterInsecurePasswordMessage,
                    Properties.Resources.msgbRegisterInsecurePasswordTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else if (newPassword != newPasswordConfirmation)
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterMismatchPasswordMessage,
                    Properties.Resources.msgbRegisterMismatchPasswordTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else
            {
                ChangeUserPasword(newPassword);
            }
        }

        private void ChangeUserPasword(string newPassword)
        {
            try
            {
                ProfileServiceClient profileServiceClient = new ProfileServiceClient();
                var response = profileServiceClient.UpdateUserPassword(Authentication.HashPassword(newPassword), DataStore.Profile.IdUser);

                if (response.StatusCode != ResponseStatus.OK)
                {
                    MessageBox.Show(ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                        ServerResponse.GetTitleFromStatusCode(response.StatusCode)
                    );
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.txtPasswordChangeSuccesfulMessage,
                        Properties.Resources.txtPasswordChangeSuccesfulTitle
                   );
                }
            }catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }
        }

        private void ImgSaveNewNicknameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string newNickname = TbNickname.Text.Trim();

            if(newNickname == "")
            {
                MessageBox.Show(
                    Properties.Resources.txtEmptyFieldNicknameMessage,
                    Properties.Resources.txtEmptyFieldNicknameTitle
                );
            }
            else if(newNickname == DataStore.Profile.NickName)
            {
                MessageBox.Show(
                    Properties.Resources.txtSameNicknameErrorMessage,
                    Properties.Resources.txtSameNicknameErrorTitle
                );
            }
            else
            {
                ValidateNewNickname(newNickname);
            }

            TbNickname.Text = DataStore.Profile.NickName;
        }

        private void ValidateNewNickname(string newNickname)
        {
            if (!Authentication.IsValidNickname(newNickname))
            {
                MessageBox.Show(
                    Properties.Resources.msgbRegisterInvalidNicknameMessage,
                    Properties.Resources.msgbRegisterInvalidNicknameTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else
            {
                try
                {
                    VerifyUnRegisteredNickname(newNickname);
                }
                catch(EndpointNotFoundException) 
                {
                    ServerResponse.ShowServerDownMessage();
                }
            }
        }

        private void VerifyUnRegisteredNickname(string newNickname)
        {
            AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
            var nicknameValidation = authenticationServiceClient.VerifyUserRegisteredByNickName(newNickname);
            if (nicknameValidation.StatusCode != ResponseStatus.OK)
            {
                MessageBox.Show(
                    ServerResponse.GetMessageFromStatusCode(nicknameValidation.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(nicknameValidation.StatusCode),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else
            {
                if (nicknameValidation.Value != null)
                {
                    MessageBox.Show(
                        Properties.Resources.msgbRegisterRegisteredNicknameMessage,
                        Properties.Resources.msgbRegisterRegisteredNicknameTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
                else
                {
                    UpdateNewUserNickname(newNickname);
                }
            }
        }

        private void UpdateNewUserNickname(string newNickname)
        {
            ProfileServiceClient profileServiceClient = new ProfileServiceClient();
            var response = profileServiceClient.UpdateUserNickname(newNickname, DataStore.Profile.IdUser);

            if(response.StatusCode != ResponseStatus.OK)
            {
                MessageBox.Show(ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(response.StatusCode)
                );
            }
            else if(response.StatusCode == ResponseStatus.OK && !response.Value)
            {
                MessageBox.Show(
                    Properties.Resources.txtNicknameChangeOutOfDateMessage,
                    Properties.Resources.txtNicknameChangeOutOfDateTitle
                );
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.txtNicknameChangedSuccesfulMessage,
                    Properties.Resources.txtNicknameChangedSuccesfulTitle
               );
                DataStore.Profile.NickName = newNickname;
            }
        }

        private void UpdateNewProfileImage(byte[] imageByteArray)
        {
            ProfileServiceClient profileServiceClient = new ProfileServiceClient();
            var response = profileServiceClient.UpdateUserProfileImage(imageByteArray, DataStore.Profile.IdUser);

            if(response.StatusCode != ResponseStatus.OK)
            {
                MessageBox.Show(ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(response.StatusCode)
                );
            }
            else
            {
                MessageBox.Show(
                    Properties.Resources.txtProfileImageChangeSuccesfulMessage,
                    Properties.Resources.txtProfileImageChangeSuccesfulTitle
                );

                DataStore.Profile.Avatar = imageByteArray;
            }
        }

        private void ImgCloseEditFullNameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HideEditFullNameFieldOptions();
            TbFullName.IsEnabled = false;
            TbFullName.Text = DataStore.Profile.FullName;
        }

        private void HideEditFullNameFieldOptions()
        {
            GridEditFullNameOptions.Visibility = Visibility.Collapsed;
            ImgEditFullNameIcon.Visibility = Visibility.Visible;
            TbFullName.Width = 274;
        }
        private void ImgEditFullNameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowEditFullNameFieldOptions();
            TbFullName.IsEnabled = true;
            TbFullName.Focus();
        }

        private void ShowEditFullNameFieldOptions()
        {
            GridEditFullNameOptions.Visibility = Visibility.Visible;
            ImgEditFullNameIcon.Visibility = Visibility.Collapsed;
            TbFullName.Width = 241;
        }

        private void ImgSaveNewFullNameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string newFullName = TbFullName.Text.Trim();

            if (newFullName == "")
            {
                MessageBox.Show(
                    Properties.Resources.txtEmptyFieldFullNameMessage,
                    Properties.Resources.txtEmptyFieldFullNameTitle
                );
            }
            else if (newFullName == DataStore.Profile.FullName)
            {
                MessageBox.Show(
                    Properties.Resources.txtSameFullNameErrorMessage,
                    Properties.Resources.txtSameFullNameErrorTitle
                );
            }
            else
            {
                UpdateNewFullName(newFullName);
            }

            TbFullName.Text = DataStore.Profile.FullName;
        }

        private void UpdateNewFullName(string newFullName)
        {
            try
            {
                ProfileServiceClient profileServiceClient = new ProfileServiceClient();
                var response = profileServiceClient.UpdateUserFullName(newFullName, DataStore.Profile.IdUser);

                if (response.StatusCode != ResponseStatus.OK)
                {
                    MessageBox.Show(ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                        ServerResponse.GetTitleFromStatusCode(response.StatusCode)
                    );
                }
                else
                {
                    MessageBox.Show(
                        Properties.Resources.txtFullNameChangeSuccesfulMessage,
                        Properties.Resources.txtFullNameChangeSuccesfulTitle
                    );

                    DataStore.Profile.FullName = newFullName;
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }
        }
        private void PbPasswordPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
        private void PbPasswordConfirmationPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void BtnReturnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
