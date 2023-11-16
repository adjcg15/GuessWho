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
using System.ServiceModel;

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
            generatedConfirmationCode = GenerateConfirmationCode(10);
            Console.WriteLine(generatedConfirmationCode);

            bool confirmationSent = SendConfirmationEmail(email, generatedConfirmationCode);
            if (!confirmationSent)
            {
                MessageBox.Show(
                    Properties.Resources.msgbConfirmEmailSendingErrorMessage,
                    Properties.Resources.msgbConfirmEmailSendingErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void BtnConfirmAccountClick(object sender, RoutedEventArgs e)
        {
            if (TbConfirmationCode.Text.Trim() != generatedConfirmationCode)
            {
                MessageBox.Show(
                    Properties.Resources.msgbWrongConfirmationCodeMessage,
                    Properties.Resources.msgbWrongConfirmationCodeTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
            var newUser = new Profile
            {
                NickName = nickname,
                Email = email,
                Password = Authentication.HashPassword(password),
                FullName = fullName,
                Avatar = profileImage
            };

            try
            {
                booleanResponse response = authenticationServiceClient.RegisterUser(newUser);
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
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(
                   Properties.Resources.msgbErrorConexionServidorMessage,
                   Properties.Resources.msgbErrorConexionServidorTitle,
                   MessageBoxButton.OK,
                   MessageBoxImage.Error
                );
            }
        }

        private static string GenerateConfirmationCode(int length)
        {
            const string CHARACTER_SET = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder confirmationCode = new StringBuilder(length);
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int characterPosition = random.Next(0, CHARACTER_SET.Length);
                confirmationCode.Append(CHARACTER_SET[characterPosition]);
            }

            return confirmationCode.ToString();
        }

        private static bool SendConfirmationEmail(string email, string code)
        {
            bool successSending = true;

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
                mailMessage.Subject = Properties.Resources.txtConfirmationEmailSubject;
                mailMessage.Body = Properties.Resources.txtConfirmationEmailBody + ": " + code;

                smtpClient.Send(mailMessage);
            } 
            catch(SmtpException ex)
            {
                successSending = false;
            }

            return successSending;
        }
    }
}
