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
            bool isValid = true;

            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                isValid = false;

                MessageBox.Show(Properties.Resources.msgbEmptyField);
            }
            if (isValid)
            {
                ValidateUserCredentials(email, password);
            }
        }

        private void ValidateUserCredentials(string email, string password)
        {
            GameServices.AuthenticationServiceClient authenticationServiceClient = new GameServices.AuthenticationServiceClient();

            try
            {
                GameServices.ProfileResponse response = authenticationServiceClient.Login(email, password);
                if (response.StatusCode == ResponseStatus.OK && response.Value != null)
                {
                    GameServices.Profile profile = response.Value;
                    MessageBox.Show(Properties.Resources.msgbWelcome1 + profile.FullName + Properties.Resources.msgbWelcome2);

                    DataStore.Profile = profile;

                    GoToMainMenuUploaded();
                }
                else if(response.StatusCode == ResponseStatus.OK && response.Value == null)
                {
                    MessageBox.Show(Properties.Resources.msgbUserNotFound);
                }
                else if (response.StatusCode == ResponseStatus.UPDATE_ERROR)
                {
                    MessageBox.Show(Properties.Resources.txtUpdateErrorMessage);
                }
                else if (response.StatusCode == ResponseStatus.VALIDATION_ERROR)
                {
                    MessageBox.Show(Properties.Resources.txtValidationErrorMessage, Properties.Resources.txtValidationErrorTitle);
                }
                else if (response.StatusCode == ResponseStatus.SQL_ERROR)
                {
                    MessageBox.Show(Properties.Resources.txtSQLErrorMessage, Properties.Resources.txtSQLErrorTitle);
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }
        }

        private void GoToMainMenuUploaded()
        {
            ShowsNavigationUI = true;
            MainMenuPage mainMenuPage = new MainMenuPage();
            this.NavigationService.Navigate(mainMenuPage);
        }
    }
}
