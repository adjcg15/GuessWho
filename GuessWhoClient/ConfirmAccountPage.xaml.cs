using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Net.Mail;
using System.Net;
using GuessWhoClient.GameServices;
using GuessWhoClient.Properties;
using System.Resources;
using GuessWhoClient.Utils;

namespace GuessWhoClient
{
    public partial class ConfirmAccountPage : Page
    {
        private string generatedConfirmationCode;
        private string nickname;
        private string email;
        private string password;
        private string fullName;
        private byte[] profileImage;

        public ConfirmAccountPage()
        {
            InitializeComponent();
        }

        public ConfirmAccountPage(string nickname, string email, string password, string fullName, byte[] profileImage)
        {
            InitializeComponent();

            this.nickname = nickname;
            this.email = email;
            this.password = password;
            this.fullName = fullName;
            this.profileImage = profileImage;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
            generatedConfirmationCode = GenerateConfirmationCode(10);
            bool confirmationSent = SendConfirmationEmail(email, this.generatedConfirmationCode);

            if (!confirmationSent)
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbConfirmEmailSendingErrorMessage"),
                    resourceManager.GetString("msgbConfirmEmailSendingErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void BtnConfirmAccountClick(object sender, RoutedEventArgs e)
        {
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);

            if (TbConfirmationCode.Text.Trim() != generatedConfirmationCode)
            {
                MessageBox.Show(
                    resourceManager.GetString("msgbWrongConfirmationCodeMessage"),
                    resourceManager.GetString("msgbWrongConfirmationCodeTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            GameServices.AuthenticationServiceClient authenticationServiceClient = new GameServices.AuthenticationServiceClient();
            var newUser = new Profile
            {
                NickName = nickname,
                Email = email,
                Password = Authentication.HashPassword(password),
                FullName = fullName,
                Avatar = profileImage
            };

            GameServices.ResponseOfboolean response = authenticationServiceClient.RegisterUser(newUser);
            bool successRegister = response.Value;
            if (!successRegister)
            {
                MessageBox.Show(
                    ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(response.StatusCode),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            MainMenuPage mainMenu = new MainMenuPage();
            this.NavigationService.Navigate(mainMenu);
        }

        private static string GenerateConfirmationCode(int length)
        {
            const string charactersSet = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder confirmationCode = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int characterPosition = random.Next(0, charactersSet.Length);
                confirmationCode.Append(charactersSet[characterPosition]);
            }

            return confirmationCode.ToString();
        }

        private static bool SendConfirmationEmail(string email, string code)
        {
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
            bool successSend = true;

            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = 587;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential("guesswhodrawn@gmail.com", "dwny hpdm zdmg jyme");

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("guesswhodrawn@gmail.com");
                mailMessage.To.Add(new MailAddress(email));
                mailMessage.Subject = resourceManager.GetString("txtConfirmationEmailSubject");
                mailMessage.Body = resourceManager.GetString("txtConfirmationEmailBody") + ": " + code;

                smtpClient.Send(mailMessage);
            } 
            catch(SmtpException ex)
            {
                successSend = false;
            }

            return successSend;
        }
    }
}
