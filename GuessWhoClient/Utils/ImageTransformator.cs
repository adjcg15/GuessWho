using GuessWhoClient.GameServices;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace GuessWhoClient.Utils
{
    public static class ImageTransformator
    {
        public static byte[] GetImageBytesFromImagePath(string imagePath)
        {
            byte[] imageBytes;

            try
            {
                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                {
                    imageBytes = Array.Empty<byte>();
                }
                else
                {
                    imageBytes = File.ReadAllBytes(imagePath);
                }
            }
            catch (IOException ex)
            {
                App.log.Error(ex.Message);

                imageBytes = Array.Empty<byte>();
            }

            return imageBytes;
        }


        public static BitmapImage GetBitmapImageFromByteArray(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                return null;
            }

            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        public static async Task<base64BinaryResponse> LoadUserAvatarLazily(string userNickname)
        {
            AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
            return await authenticationServiceClient.GetAvatarAsync(userNickname);
        }
    }
}
