using GuessWhoClient.Properties;
using GuessWhoClient.Utils;
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
            tbEmail.Text = "";
            pbPassword.Password = "";
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

                ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
                MessageBox.Show(resourceManager.GetString("msgbEmptyField"));
            }
            if (isValid)
            {
                ValidateUserCredentials(email, password);
            }
        }

        private void ValidateUserCredentials(string email, string password)
        {
            GameServices.UserServiceClient userServiceClient = new GameServices.UserServiceClient();
            GameServices.Profile profile = userServiceClient.Login(email, password);

            if (profile != null)
            {
                ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
                MessageBox.Show(resourceManager.GetString("msgbWelcome1") + profile.FullName + resourceManager.GetString("msgbWelcome2"));

                ProfileSingleton.Instance = profile;
                GoToMainMenuUploaded();
            }
            else
            {
                ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
                MessageBox.Show(resourceManager.GetString("msgbUserNotFound") +" "+ resourceManager.GetString("msgbConectionLost"));
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
