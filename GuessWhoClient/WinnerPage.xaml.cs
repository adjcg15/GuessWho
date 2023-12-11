﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuessWhoClient
{
    /// <summary>
    /// Lógica de interacción para WinnerPage.xaml
    /// </summary>
    public partial class WinnerPage : Page
    {
        public WinnerPage()
        {
            InitializeComponent();
        }

        public void InitializeWinnerTextBlock(string winnerNickname)
        {
            TbWinner.Text = winnerNickname;
        }

        private void BtnMainMenuClick(object sender, RoutedEventArgs e)
        {
            MainMenuPage mainMenuPage = new MainMenuPage();
            NavigationService.Navigate(mainMenuPage);
        }
    }
}
