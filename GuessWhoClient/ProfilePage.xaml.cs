using GuessWhoClient.Utils;
using System;
using System.Windows;
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

        private void ImgEditNicknameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowEditNicknameFieldOptions();
            TbNickname.IsEnabled = true;
            TbNickname.Focus();
        }

        private void ImgCloseEditNicknameClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HideEditNicknameFieldOptions();
            TbNickname.IsEnabled = false;
            TbNickname.Text = DataStore.Profile.NickName;
        }

        private void ShowEditNicknameFieldOptions()
        {
            GridEditNicknameOptions.Visibility = Visibility.Visible;
            ImgEditNicknameIcon.Visibility = Visibility.Collapsed;
            TbNickname.Width = 241;
        }

        private void HideEditNicknameFieldOptions()
        {
            GridEditNicknameOptions.Visibility = Visibility.Collapsed;
            ImgEditNicknameIcon.Visibility = Visibility.Visible;
            TbNickname.Width = 274;
        }
    }
}
