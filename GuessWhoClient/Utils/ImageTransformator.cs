using GuessWhoClient.GameServices;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace GuessWhoClient.Utils
{
    public class ImageTransformator
    {
        public static byte[] GetImageBytesFromImagePath(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(imagePath);
            } 
            catch(IOException)
            {
                return null;
            }
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
