using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoClient.Utils
{
    public class Email
    {
        private static readonly string HOST = "smtp.gmail.com";
        private static readonly int PORT = 587;
        private static readonly string USERNAME = "guesswhodrawn@gmail.com";
        private static readonly string PASSWORD = "dwny hpdm zdmg jyme";
        private static readonly string SENDER_MAIL_ADDRESS = "guesswhodrawn@gmail.com";

        public static bool SendMail(string email, string subject, string body)
        {
            bool successSending = true;

            try
            {
                SmtpClient smtpClient = new SmtpClient(HOST);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = PORT;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential(USERNAME, PASSWORD);

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(SENDER_MAIL_ADDRESS);
                mailMessage.To.Add(new MailAddress(email));
                mailMessage.Subject = subject;
                mailMessage.Body = body;

                smtpClient.Send(mailMessage);
            }
            catch (SmtpException)
            {
                successSending = false;
            }

            return successSending;
        }
    }
}
