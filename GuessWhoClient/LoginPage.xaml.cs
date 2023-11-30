using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
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
            string email = tbEmail.Text.Trim();
            string password = Authentication.HashPassword(pbPassword.Password.Trim());

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
