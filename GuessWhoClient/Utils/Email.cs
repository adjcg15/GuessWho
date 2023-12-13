using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace GuessWhoClient.Utils
{
    public static class Email
    {
        public static bool SendMail(string email, string subject, string body)
        {
            bool successSending = true;

            try
            {
                SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["HOST"]);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Port = int.Parse(ConfigurationManager.AppSettings["PORT"]);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["USERNAME"], ConfigurationManager.AppSettings["PASSWORD"]);

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SENDER_EMAIL_ADDRESS"]);
                mailMessage.To.Add(new MailAddress(email));
                mailMessage.Subject = subject;
                mailMessage.Body = body;

                smtpClient.Send(mailMessage);
            }
            catch (SmtpException ex)
            {
                successSending = false;

                App.log.Warn(ex.Message);
            }

            return successSending;
        }
    }
}
