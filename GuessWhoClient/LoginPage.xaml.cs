﻿using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            string email = TbEmail.Text.Trim();
            string password = Authentication.HashPassword(PbPassword.Password.Trim());

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
                var emailVerification = authenticationServiceClient.VerifyUserRegisteredByEmail(email);

                switch(emailVerification.StatusCode)
                {
                    case ResponseStatus.OK:
                        if(emailVerification.Value != null)
                        {
                            StablishUserSession(email, password);
                        }
                        else
                        {
                            MessageBox.Show(
                                Properties.Resources.msgbNonExistentAccountMessage,
                                Properties.Resources.msgbNonExistentAccountTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning
                            );
                        }
                        break;
                    case ResponseStatus.SQL_ERROR:
                        MessageBox.Show(
                            ServerResponse.GetMessageFromStatusCode(ResponseStatus.SQL_ERROR),
                            ServerResponse.GetTitleFromStatusCode(ResponseStatus.SQL_ERROR),
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
                RedirectToMainMenu();
            }
            catch(CommunicationException ex)
            {
                App.log.Error(ex.Message);

                ServerResponse.ShowConnectionLostMessage();
                RedirectToMainMenu();
            }
        }

        private void StablishUserSession(string email, string password)
        {
            if (!CheckPlayerIsPermanentBanned(email) && !CheckPlayerIsTemporarilyBanned(email))
            {
                AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
                ProfileResponse userProfile = authenticationServiceClient.Login(email, password);

                switch (userProfile.StatusCode)
                {
                    case ResponseStatus.OK:
                        MessageBox.Show(Properties.Resources.msgbWelcome1 + userProfile.Value.FullName + Properties.Resources.msgbWelcome2);
                        DataStore.Profile = userProfile.Value;
                        RedirectToMainMenu();
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
                            ServerResponse.GetMessageFromStatusCode(userProfile.StatusCode),
                            ServerResponse.GetTitleFromStatusCode(userProfile.StatusCode),
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        RedirectToMainMenu();
                        break;
                }
            }
        }

        private bool CheckPlayerIsPermanentBanned(string playerEmail)
        {
            bool permanentBanned = true;

            ReportServiceClient reportServiceClient = new ReportServiceClient();
            var response = reportServiceClient.VerifyPlayerPermanentBanned(playerEmail);

            switch (response.StatusCode)
            {
                case ResponseStatus.OK:
                    if (!response.Value)
                    {
                        permanentBanned = false;
                    }
                    else
                    {
                        MessageBox.Show(
                            Properties.Resources.msgbPermanentBanMessage,
                            Properties.Resources.msgbPermanentBanTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                    break;
                case ResponseStatus.VALIDATION_ERROR:
                case ResponseStatus.SQL_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbErrorVerifyingBanMessage,
                        Properties.Resources.msgbErrorVerifyingBanTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
            }

            return permanentBanned;
        }

        private bool CheckPlayerIsTemporarilyBanned(string playerEmail)
        {
            bool temporarilyBanned = true;

            ReportServiceClient reportServiceClient = new ReportServiceClient();
            var response = reportServiceClient.VerifyPlayerTemporarilyBanned(playerEmail);

            switch (response.StatusCode)
            {
                case ResponseStatus.OK:
                    temporarilyBanned = false;
                    break;
                case ResponseStatus.NOT_ALLOWED:
                    if (response.Value == DateTime.MinValue)
                    {
                        MessageBox.Show(
                            Properties.Resources.msgbTemporalBanGenericMessage,
                            Properties.Resources.msgbTemporalBanTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                    else
                    {
                        DateTime lockEndDate = response.Value;
                        string parsedLockEndDate = lockEndDate.ToString("dd/MM/yyyy");

                        MessageBox.Show(
                            Properties.Resources.msgbTemporalBanMessage + " " + parsedLockEndDate,
                            Properties.Resources.msgbTemporalBanTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                    break;
                case ResponseStatus.VALIDATION_ERROR:
                case ResponseStatus.SQL_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbErrorVerifyingBanMessage,
                        Properties.Resources.msgbErrorVerifyingBanTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
            }

            return temporarilyBanned;
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

        private void PbPasswordPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void BtnReturnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
