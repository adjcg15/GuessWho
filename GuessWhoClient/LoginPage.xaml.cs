using GuessWhoClient.GameServices;
using GuessWhoClient.Properties;
using GuessWhoClient.Utils;
using System;
using System.Resources;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void BtnLoginClick(object sender, RoutedEventArgs e)
        {
            ValidateFields();
        }

        private void BtnSignUpClick(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            this.NavigationService.Navigate(registerPage);
        }

        private void ValidateFields()
        {
            string email = tbEmail.Text.Trim();
            string password = Authentication.HashPassword(pbPassword.Password.Trim());

            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(Properties.Resources.msgbEmptyField);
            }
            else
            {
                ValidateUserCredentials(email, password);
            }
        }

        private void ValidateUserCredentials(string email, string password)
        {
            try
            {
                AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
                ProfileResponse response = authenticationServiceClient.Login(email, password);

                switch (response.StatusCode)
                {
                    case ResponseStatus.OK:
                        if (response.Value != null)
                        {
                            Profile profile = response.Value;
                            MessageBox.Show(Properties.Resources.msgbWelcome1 + profile.FullName + Properties.Resources.msgbWelcome2);
                            DataStore.Profile = profile;
                            RedirectToMainMenu();
                        }
                        else
                        {
                            MessageBox.Show(Properties.Resources.msgbUserNotFound);
                        }
                        break;
                    case ResponseStatus.UPDATE_ERROR:
                        MessageBox.Show(Properties.Resources.txtUpdateErrorMessage);
                        break;
                    case ResponseStatus.VALIDATION_ERROR:
                        MessageBox.Show(Properties.Resources.txtValidationErrorMessage, Properties.Resources.txtValidationErrorTitle);
                        break;
                    case ResponseStatus.SQL_ERROR:
                        MessageBox.Show(Properties.Resources.txtSQLErrorMessage, Properties.Resources.txtSQLErrorTitle);
                        break;
                    default:
                        MessageBox.Show(
                            ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                            ServerResponse.GetTitleFromStatusCode(response.StatusCode),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        RedirectToMainMenu();
                        break;
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }
        }

        private void RedirectToMainMenu()
        {
            ShowsNavigationUI = true;
            MainMenuPage mainMenuPage = new MainMenuPage();
            this.NavigationService.Navigate(mainMenuPage);
        }
    }
}
