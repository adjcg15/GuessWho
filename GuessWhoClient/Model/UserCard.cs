using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public class UserCard : INotifyPropertyChanged
    {
        private string nickname;
        private BitmapImage avatar;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Nickname { get { return nickname; } set { nickname = value; } }

        public BitmapImage Avatar
        {
            get { return avatar; }
            set
            {
                if (avatar != value)
                {
                    avatar = value;
                    OnPropertyChanged(nameof(Avatar));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
