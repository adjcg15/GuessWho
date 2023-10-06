using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Net.Mail;
using System.Net;

namespace GuessWhoClient
{
    public partial class ConfirmAccountPage : Page
    {
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
            string confirmationCode = GenerateConfirmationCode(10);
            bool confirmationSent = SendConfirmationEmail(email, confirmationCode);

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
                SmtpClient smtpClient = new SmtpClient("localhost");
                smtpClient.Port = 25;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("noreply@guesswho.com");
                mailMessage.To.Add(new MailAddress(email));
                mailMessage.Subject = "Confirmación de correo electrónico";
                mailMessage.Body = "Tu código de confirmación: " + code;

                smtpClient.Send(mailMessage);
            } 
            catch(SmtpException ex)
            {
                Console.WriteLine(ex.StackTrace);
                successSend = false;
            }

            return successSend;
        }
    }
}
