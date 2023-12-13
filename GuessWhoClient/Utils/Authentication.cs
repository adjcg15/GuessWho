using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuessWhoClient.Utils
{
    public static class Authentication
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
        public static bool IsValidEmail(string email)
        {
            bool isValidEmail;
            if(string.IsNullOrEmpty(email))
            {
                isValidEmail = false;
            } 
            else
            {
                try
                {
                    new MailAddress(email);
                    isValidEmail = true;
                }
                catch (FormatException)
                {
                    isValidEmail = false;
                }
            }

            return isValidEmail;
        }

        public static bool IsSecurePassword(string password)
        {
            bool isSecurePassword;
            string pattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?#\-&_])[A-Za-z\d@$!%*?#\-&_]{8,}$";

            TimeSpan timeout = TimeSpan.FromMilliseconds(500);

            try
            {
                isSecurePassword = Regex.IsMatch(password, pattern, RegexOptions.None, timeout);
            }
            catch (RegexMatchTimeoutException ex)
            {
                App.log.Warn(ex.Message);

                isSecurePassword = false;
            }

            return isSecurePassword;
        }

        public static bool IsValidNickname(string nickname)
        {
            bool isValidNickname;
            string pattern = @"^[a-zA-Z0-9_]+$";

            TimeSpan timeout = TimeSpan.FromMilliseconds(500);

            try
            {
                isValidNickname = Regex.IsMatch(nickname, pattern, RegexOptions.None, timeout);
            }
            catch (RegexMatchTimeoutException ex)
            {
                App.log.Warn(ex.Message);

                isValidNickname = false;
            }

            return isValidNickname;
        }
    }
}
