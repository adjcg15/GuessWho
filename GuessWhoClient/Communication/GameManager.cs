using GuessWhoClient.GameServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public class GameManager : IGameServiceCallback
    {
        private static GameManager instance;
        private GameServiceClient client;
        private List<IGamePage> subscribedPages = new List<IGamePage>();
        private string currentMatchCode;
        private bool isCurrentMatchHost;
        private string adversaryNickname;
        private byte[] adversaryAvatar;
        private Character selectedCharacter;
        private List<Character> charactersInGame;

        private GameManager() { }

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }
                return instance;
            }
        }

        public GameServiceClient Client 
        { 
            get 
            { 
                if (client == null)
                {
                    client = new GameServiceClient(new InstanceContext(this));
                }

                return client; 
            } 
        }
        public string CurrentMatchCode { get { return currentMatchCode; } set { currentMatchCode = value; } }

        public bool IsCurrentMatchHost { get { return isCurrentMatchHost; } set { isCurrentMatchHost = value; } }

        public string AdversaryNickname { get { return adversaryNickname; } set { adversaryNickname = value; } }

        public byte[] AdversaryAvatar { get { return adversaryAvatar; } set { adversaryAvatar = value; } }

        public Character SelectedCharacter { get { return selectedCharacter; } set { selectedCharacter = value; } }

        public List<Character> CharactersInGame
        {
            get
            {
                if(charactersInGame == null)
                {
                    charactersInGame = InitializeCharacters();
                }

                return charactersInGame;
            }
            set {  charactersInGame = value; }
        }

        private List<Character> InitializeCharacters()
        {
            List<Character> charactersList = new List<Character>();

            string PROJECT_DIRECTORY = Path.Combine(AppContext.BaseDirectory, "..\\..\\");
            string CHARACTERS_FOLDER = Path.Combine(PROJECT_DIRECTORY, "Resources\\Characters");
            string[] imageFiles = Directory.GetFiles(CHARACTERS_FOLDER, "*.png");

            foreach (string imagePath in imageFiles)
            {
                Character character = new Character
                {
                    IsSelected = false,
                    Avatar = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                    Name = Path.GetFileNameWithoutExtension(imagePath)
                };
                charactersList.Add(character);
            }

            return charactersList;
        }

        public void SubscribePage(IGamePage page)
        {
            if (!subscribedPages.Contains(page))
            {
                subscribedPages.Add(page);
            }
        }

        public void UnsubscribePage(IGamePage page)
        {
            subscribedPages.Remove(page);
        }

        public void RestartRawValues()
        {
            isCurrentMatchHost = false;
            currentMatchCode = "";
            adversaryAvatar = null;
            adversaryNickname = "";
            client = null;
            subscribedPages = new List<IGamePage>();
            selectedCharacter = null;
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {
            List<IGamePage> pagesCopy = new List<IGamePage>(subscribedPages);

            foreach (var page in pagesCopy)
            {
                page.PlayerStatusInMatchChanged(player, isInMatch);
            }
        }
    }
}
