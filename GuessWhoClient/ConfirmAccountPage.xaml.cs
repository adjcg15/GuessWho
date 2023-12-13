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
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;

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

                switch (response.StatusCode)
                {
                    case ResponseStatus.OK:
                        MessageBox.Show(
                            Properties.Resources.msgbUserRegisteredSuccessfullyMessage,
                             Properties.Resources.msgbUserRegisteredSuccessfullyTitle,
                             MessageBoxButton.OK,
                             MessageBoxImage.Information
                        );
                        break;
                    case ResponseStatus.NOT_ALLOWED:
                        MessageBox.Show(
                            Properties.Resources.msgbUserAlreadyRegisteredMessage,
                            Properties.Resources.msgbUserAlreadyRegisteredTitle,
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
                        break;
                }
            }
            catch (EndpointNotFoundException ex)
            {
                App.log.Fatal(ex.Message);

                ServerResponse.ShowServerDownMessage();
            }
            catch(CommunicationException ex) 
            {
                App.log.Error(ex.Message);

                ServerResponse.ShowConnectionLostMessage();
            }
            finally
            {
                MainMenuPage mainMenu = new MainMenuPage();
                this.NavigationService.Navigate(mainMenu);
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
            return Email.SendMail(email, Properties.Resources.txtConfirmationEmailSubject, (Properties.Resources.txtConfirmationEmailBody + ": " + code));
        }

        private void TbConfirmationCodeTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string input = textBox.Text;

            string filteredInput = new string(input.Where(char.IsLetterOrDigit).ToArray());

            textBox.Text = filteredInput;

            textBox.CaretIndex = filteredInput.Length;
        }
    }
}
