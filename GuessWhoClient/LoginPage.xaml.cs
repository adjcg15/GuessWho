using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
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
            string email = TbEmail.Text.Trim();
            string password = Authentication.HashPassword(PbPassword.Password.Trim());

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show(
                    Properties.Resources.msgbEmptyFieldMessage,
                    Properties.Resources.msgbEmptyFieldTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            else
            {
                if (!CheckPlayerIsPermanentBanned(email))
                {
                    if (!CheckPlayerIsTemporarilyBanned(email))
                    {
                        ValidateUserCredentials(email, password);
                    }
                }
            }
        }

        private bool CheckPlayerIsPermanentBanned(string playerEmail)
        {
            bool permanentBanned = true;

            try
            {
                ReportServiceClient reportServiceClient = new ReportServiceClient();
                var response = reportServiceClient.VerifyPlayerPermanentBanned(playerEmail);

                switch(response.StatusCode)
                {
                    case ResponseStatus.OK:
                        if (!response.Value)
                        {
                            permanentBanned = false;
                        }
                        else
                        {
                            MessageBox.Show(
                                Properties.Resources.msgbPermanentBanMessage,
                                Properties.Resources.msgbPermanentBanTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                        }
                        break;
                    case ResponseStatus.VALIDATION_ERROR:
                    case ResponseStatus.SQL_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbErrorVerifyingBanMessage,
                            Properties.Resources.msgbErrorVerifyingBanTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        break;
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }

            return permanentBanned;
        }

        private bool CheckPlayerIsTemporarilyBanned(string playerEmail)
        {
            bool temporarilyBanned = true;

            try
            {
                ReportServiceClient reportServiceClient = new ReportServiceClient();
                var response = reportServiceClient.VerifyPlayerTemporarilyBanned(playerEmail);

                switch (response.StatusCode)
                {
                    case ResponseStatus.OK:
                        temporarilyBanned = false;
                        break;
                    case ResponseStatus.NOT_ALLOWED:
                        if (response.Value == DateTime.MinValue)
                        {
                            MessageBox.Show(
                                Properties.Resources.msgbTemporalBanGenericMessage,
                                Properties.Resources.msgbTemporalBanTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                        }
                        else
                        {
                            DateTime lockEndDate = response.Value;
                            string parsedLockEndDate = lockEndDate.ToString("dd/MM/yyyy");

                            MessageBox.Show(
                                Properties.Resources.msgbTemporalBanMessage + " " + parsedLockEndDate,
                                Properties.Resources.msgbTemporalBanTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
                        }
                        break;
                    case ResponseStatus.VALIDATION_ERROR:
                    case ResponseStatus.SQL_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbErrorVerifyingBanMessage,
                            Properties.Resources.msgbErrorVerifyingBanTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        break;
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }

            return temporarilyBanned;
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
                        break;
                    case ResponseStatus.VALIDATION_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbInvalidCredentialsMessage,
                            Properties.Resources.msgbInvalidCredentialsTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        break;
                    case ResponseStatus.NOT_ALLOWED:
                        MessageBox.Show(
                            Properties.Resources.msgbInvalidSessionMessage,
                            Properties.Resources.msgbInvalidSessionTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
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
                RedirectToMainMenu();
            }
        }

        private void BtnSignUpClick(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            NavigationService.Navigate(registerPage);
        }

        private void RedirectToMainMenu()
        {
            MainMenuPage mainMenuPage = new MainMenuPage();
            this.NavigationService.Navigate(mainMenuPage);
        }
    }
}
