using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class ChooseCharacterPage : Page, IGamePage, IMatchStatusListener
    {
        private Character selectedCharacter;
        private GameManager gameManager = GameManager.Instance;
        private MatchStatusManager matchStatusManager = MatchStatusManager.Instance;

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

            string PROJECT_DIRECTORY = System.IO.Path.Combine(AppContext.BaseDirectory, "..\\..\\");
            string CHARACTERS_FOLDER = System.IO.Path.Combine(PROJECT_DIRECTORY, "Resources\\Characters");
            string[] imageFiles = Directory.GetFiles(CHARACTERS_FOLDER, "*.png");

            foreach (string imagePath in imageFiles)
            {
                Character character = new Character
                {
                    Avatar = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Name = System.IO.Path.GetFileNameWithoutExtension(imagePath),
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
            }
        }

        private void UpdateUI(Character character)
        {
            LbCharacter.Content = character.Name;
            LbCharacter.Foreground = (SolidColorBrush)FindResource("BlueBrush");
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
            throw new NotImplementedException();
        }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            throw new NotImplementedException();
        }
    }
}
