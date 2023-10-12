﻿using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class MainMenuPage : Page
    {

        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void BtnLoginClick(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            this.NavigationService.Navigate(loginPage);
        }

        private void BtnRegisterClick(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            this.NavigationService.Navigate(registerPage);
        }

        private void BtnLogOutClick(object sender, RoutedEventArgs e)
        {
            ProfileSingleton.Instance = null;

            BtnLogOut.Visibility = Visibility.Collapsed;
            BtnTournamentMatch.Visibility = Visibility.Collapsed;
            BtnLogin.Visibility = Visibility.Visible;
            BtnRegister.Visibility = Visibility.Visible;
            BorderProfile.Visibility = Visibility.Collapsed;
            BtnLeaderboard.Visibility = Visibility.Collapsed;
            BtnFriends.Visibility = Visibility.Collapsed;
        }

        private void BtnQuickMatchClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnLeaderboardClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnJoinMatchClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnHowToPlayClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTournamentMatchClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnFriendsClick(object sender, RoutedEventArgs e)
        {

        }

        private void cbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = cbLanguage.SelectedIndex;

            if (selectedIndex == 0)
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-MX");
            }
            else if (selectedIndex == 1)
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            }

            ReloadLanguageResources();
        }

        public void LoginProfile()
        {
            BtnLogOut.Visibility = Visibility.Visible;
            BtnTournamentMatch.Visibility = Visibility.Visible;
            BtnLogin.Visibility = Visibility.Collapsed;
            BtnRegister.Visibility = Visibility.Collapsed;
            BorderProfile.Visibility = Visibility.Visible;
            BtnLeaderboard.Visibility = Visibility.Visible;
            BtnFriends.Visibility = Visibility.Visible;

            lbNickname.Content = ProfileSingleton.Instance.NickName;

            BitmapImage profileImageSrc = ImageTransformator.GetBitmapImageFromByteArray(ProfileSingleton.Instance.Avatar);
            if (profileImageSrc != null)
            {
                ImgProfilePicture.Source = profileImageSrc;
            }
        }

        private void ReloadLanguageResources()
        {
            TbBtnFriends.Text = Properties.Resources.btnFriends;
            TbBtnJoinMatch.Text = Properties.Resources.btnJoinMatch;
            TbBtnLeaderboard.Text = Properties.Resources.btnLeaderboard;
            TbBtnLogin.Text = Properties.Resources.txtLoginGlobal;
            TbBtnLogOut.Text = Properties.Resources.btnLogOut;
            TbBtnQuickMatch.Text = Properties.Resources.btnQuickMatch;
            TbBtnRegister.Text = Properties.Resources.txtSignUpGlobal;
            TbBtnTournamentMatch.Text = Properties.Resources.btnTournamentMatch;
        }
    }
}
