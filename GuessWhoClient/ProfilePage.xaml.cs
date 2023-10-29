using GuessWhoClient.Utils;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class ProfilePage : Page
    {
        private const string DEFAULT_PROFILE_PICTURE_ROUTE = "pack://application:,,,/Resources/user-icon.png";

        public ProfilePage()
        {
            InitializeComponent();
            StablishUIValuesFromProfileInfo();
        }

        private void StablishUIValuesFromProfileInfo()
        {
            LbEmail.Content = DataStore.Profile.Email;
            LbFullName.Content = DataStore.Profile.FullName;
            TbNickname.Text = DataStore.Profile.NickName;
            PbPassword.Password = DataStore.Profile.Password;
            PbPasswordConfirmation.Password = DataStore.Profile.Password;

            BitmapImage bitmapAvatar = ImageTransformator.GetBitmapImageFromByteArray(DataStore.Profile.Avatar);
            if(bitmapAvatar != null)
            {
                ImgAvatar.Source = bitmapAvatar;
            } 
            else
            {
                Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
                BitmapImage defaultImage = new BitmapImage(uri);
                ImgAvatar.Source = defaultImage;
            }
        }
    }
}
