using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Net.Mail;
using System.Net;
using GuessWhoClient.GameServices;

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
            bool confirmationSent = SendConfirmationEmail(email, this.generatedConfirmationCode);

            if(!confirmationSent)
            {
                MessageBox.Show(
                    "Ocurrió un error al enviar el correo electrónico, intente registrarse en otro momento",
                    "Error de envío",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void BtnConfirmAccountClick(object sender, RoutedEventArgs e)
        {
            if(TbConfirmationCode.Text.Trim() != generatedConfirmationCode)
            {
                MessageBox.Show(
                    "El código introducido no coincide con el código generado",
                    "Verifique el código ingresado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            GameServices.UserServiceClient userServiceClient = new GameServices.UserServiceClient();
            var newUser = new Profile
            {
                NickName = nickname,
                Email = email,
                Password = password,
                FullName = fullName,
                Avatar = profileImage
            };
            bool registered = userServiceClient.RegisterUser(newUser);

            if(!registered)
            {
                MessageBox.Show(
                    "No fue posible completar su registro, por favor intente de nuevo más tarde",
                    "Error de registro",
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
                mailMessage.Subject = "Confirmación de correo electrónico";
                mailMessage.Body = "Tu código de confirmación: " + code;

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
