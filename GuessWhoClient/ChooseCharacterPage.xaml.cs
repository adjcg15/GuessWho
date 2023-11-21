using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    public class Character : INotifyPropertyChanged
    {
        private BitmapImage avatar;
        private string name;
        private bool isSelected;

        private SolidColorBrush borderBrush;

        public SolidColorBrush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (borderBrush != value)
                {
                    borderBrush = value;
                    OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }

        public BitmapImage Avatar { get { return avatar; } set { avatar = value; } }
        public string Name { get { return name; } set { name = value; } }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /// <summary>
    /// Lógica de interacción para ChooseCharacterPage.xaml
    /// </summary>
    /// 
    public partial class ChooseCharacterPage : Page
    {
        private Character selectedCharacter;

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
                selectedCharacter.BorderBrush = null;
                selectedCharacter = null;

                LbCharacter.Foreground = Brushes.Black;
            }
        }

        private void UpdateUI(Character character)
        {
            character.BorderBrush = (SolidColorBrush)FindResource("BlueBrush");
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
    }
}
