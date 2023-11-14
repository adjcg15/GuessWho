using GuessWhoClient.Properties;
using GuessWhoClient.Utils;
using System;
using System.Resources;
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
            GameServices.ProfileResponse response = authenticationServiceClient.Login(email, password);

            if (response.Value != null)
            {
                GameServices.Profile profile = response.Value;
                MessageBox.Show(Properties.Resources.msgbWelcome1 + profile.FullName + Properties.Resources.msgbWelcome2);

                Console.WriteLine(profile.NickName);
                DataStore.Profile = profile;

                Console.WriteLine(DataStore.Profile?.NickName);
                GoToMainMenuUploaded();
            }
            else if (response.StatusCode == GameServices.ResponseStatus.UPDATE_ERROR)
            {
                MessageBox.Show(Properties.Resources.txtUpdateErrorMessage);
            }
            else if(response.StatusCode == GameServices.ResponseStatus.VALIDATION_ERROR)
            {
                MessageBox.Show(Properties.Resources.txtValidationErrorMessage, Properties.Resources.txtValidationErrorTitle);
            }
            else if(response.StatusCode == GameServices.ResponseStatus.SQL_ERROR)
            {
                MessageBox.Show(Properties.Resources.txtSQLErrorMessage, Properties.Resources.txtSQLErrorTitle);
            }
            else
            {
                MessageBox.Show(Properties.Resources.msgbUserNotFound);
            } 
        }

        private void GoToMainMenuUploaded()
        {
            MainMenuPage mainMenuPage = new MainMenuPage();
            mainMenuPage.LoginProfile();
            this.NavigationService.Navigate(mainMenuPage);
        }
    }
}
