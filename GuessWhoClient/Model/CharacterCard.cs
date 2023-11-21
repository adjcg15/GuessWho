using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public class Character : INotifyPropertyChanged
    {
        private BitmapImage avatar;
        private bool isSelected;
        private string name;
        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
