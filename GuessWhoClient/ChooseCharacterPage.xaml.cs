using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class ChooseCharacterPage : Page, IGamePage, IMatchStatusPage
    {
        private Character selectedCharacter;
        private GameManager gameManager = GameManager.Instance;
        private MatchStatusManager matchStatusManager = MatchStatusManager.Instance;
        private bool isGuestReady = false;

        public ChooseCharacterPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ShowCharacters();
        }

        private void ShowCharacters()
        {
            IcCharacters.ItemsSource = RecoverCharacters();
        }

        private List<Character> RecoverCharacters()
        {
            List<Character> imageList = new List<Character>();

            string PROJECT_DIRECTORY = Path.Combine(AppContext.BaseDirectory, "..\\..\\");
            string CHARACTERS_FOLDER = Path.Combine(PROJECT_DIRECTORY, "Resources\\Characters");
            string[] imageFiles = Directory.GetFiles(CHARACTERS_FOLDER, "*.png");

            foreach (string imagePath in imageFiles)
            {
                Character character = new Character
                {
                    Avatar = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Name = Path.GetFileNameWithoutExtension(imagePath),
                };
                imageList.Add(character);
            }

            return imageList;
        }

        private void BorderCharacterClick(object sender, RoutedEventArgs e)
        {
            var clickedCharacter = (sender as Border)?.DataContext as Character;

            if (clickedCharacter != null)
            {
                if (clickedCharacter.IsSelected)
                {
                    clickedCharacter.IsSelected = false;
                    DeselectCharacter();
                }
                else
                {
                    DeselectCharacter();
                    clickedCharacter.IsSelected = true;
                    selectedCharacter = clickedCharacter;

                    UpdateUI(clickedCharacter);
                }
            }
        }

        private void DeselectCharacter()
        {
            if (selectedCharacter != null)
            {
                selectedCharacter.IsSelected = false;
                selectedCharacter = null;

                LbCharacter.Foreground = Brushes.Black;

                DisableBtnConfirmCharacterSelection();
            }
        }

        private void DisableBtnConfirmCharacterSelection()
        {
            BtnConfirmCharacterSelection.Opacity = 0.5;
            BtnConfirmCharacterSelection.IsEnabled = false;
        }

        private void UpdateUI(Character character)
        {
            LbCharacter.Content = character.Name;
            LbCharacter.Foreground = (SolidColorBrush)FindResource("BlueBrush");

            BtnConfirmCharacterSelection.Opacity = 1;
            BtnConfirmCharacterSelection.IsEnabled = true;
        }

        private void BorderCharacterMouseEnter(object sender, MouseEventArgs e)
        {
            var enteredCharacter = (sender as Border)?.DataContext as Character;

            if (enteredCharacter != null)
            {
                LbCharacter.Content = enteredCharacter.Name;

                if (enteredCharacter == selectedCharacter)
                {
                    LbCharacter.Foreground = (SolidColorBrush)FindResource("BlueBrush");
                }
                else
                {
                    LbCharacter.Foreground = Brushes.Black;
                }
            }
        }

        private void BorderCharacterMouseLeave(object sender, MouseEventArgs e)
        {
            LbCharacter.Content = selectedCharacter?.Name;
            LbCharacter.Foreground = (SolidColorBrush)FindResource("BlueBrush");
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {
            NotifyGameHasBeenCanceled();
        }

        private void NotifyGameHasBeenCanceled()
        {
            ClearCommunicationChannels();
            RedirectPermanentlyToMainMenu();
        }

        private void ClearCommunicationChannels()
        {
            gameManager.RestartRawValues();
            matchStatusManager.RestartRawValues();
        }

        private void RedirectPermanentlyToMainMenu()
        {
            MainMenuPage mainMenu = new MainMenuPage();
            mainMenu.ShowCanceledMatchMessage();
            this.NavigationService.Navigate(mainMenu);
        }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            Console.WriteLine(matchStatusCode + " " + gameManager.IsCurrentMatchHost + " " + selectedCharacter == null);

            if (matchStatusCode == MatchStatus.PlayerReady && gameManager.IsCurrentMatchHost && selectedCharacter != null)
            {
                BtnConfirmCharacterSelection.Visibility = Visibility.Collapsed;
                BtnStartGame.Visibility = Visibility.Visible;

                isGuestReady = true;
            }
            else if (matchStatusCode == MatchStatus.PlayerReady && gameManager.IsCurrentMatchHost && selectedCharacter == null)
            {
                isGuestReady = true;
            }
            else if (matchStatusCode == MatchStatus.StartGame && !gameManager.IsCurrentMatchHost)
            {
                RedirectToDrawingPage();
            }
        }

        private void RedirectToDrawingPage()
        {
            gameManager.UnsubscribePage(this);
            matchStatusManager.UnsubscribePage(this);

            DrawingPage drawingPage = new DrawingPage();
            NavigationService.Navigate(drawingPage);
            gameManager.SubscribePage(drawingPage);
            matchStatusManager.SubscribePage(drawingPage);
        }

        private void BtnStartGameClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(selectedCharacter);
            try
            {
                if (selectedCharacter != null && gameManager.IsCurrentMatchHost && isGuestReady)
                {
                    matchStatusManager.Client.StartGame(selectedCharacter.Name, matchStatusManager.CurrentMatchCode);
                    RedirectToDrawingPage();
                }
            }
            catch (EndpointNotFoundException ex)
            {
                Utils.ServerResponse.ShowServerDownMessage();
                //TO-DO Ex log
            }
        }

        private void BtnConfirmCharacterSelectionClick(object sender, RoutedEventArgs e)
        {
            IcCharacters.IsEnabled = false;
            DisableBtnConfirmCharacterSelection();

            try
            {
                ConfirmSelectedCharacter();
            }
            catch (EndpointNotFoundException ex)
            {
                ServerResponse.ShowServerDownMessage();
                //TO-DO Ex log
            } 
        }

        private void ConfirmSelectedCharacter()
        {
            if (selectedCharacter != null && !gameManager.IsCurrentMatchHost) 
            {
                matchStatusManager.Client.SelectCharacter(selectedCharacter.Name, gameManager.CurrentMatchCode);
                BtnConfirmCharacterSelection.Content = Properties.Resources.lbWaitingOpponent;
            }
            else if (selectedCharacter != null && gameManager.IsCurrentMatchHost && isGuestReady)
            {
                BtnConfirmCharacterSelection.Visibility = Visibility.Collapsed;
                BtnStartGame.Visibility = Visibility.Visible;
            }
            else if (selectedCharacter != null && gameManager.IsCurrentMatchHost && !isGuestReady) 
            {
                DisableBtnConfirmCharacterSelection();
                BtnConfirmCharacterSelection.Content = Properties.Resources.lbWaitingOpponent;
            }
        }
    }
}
