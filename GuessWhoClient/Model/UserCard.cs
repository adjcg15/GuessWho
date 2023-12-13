using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public class UserCard : INotifyPropertyChanged
    {
        private BitmapImage avatar;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Nickname { get; set; }

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
