using System.IO;
using System.Windows.Media.Imaging;

namespace GuessWhoClient.Utils
{
    public class ImageTransformator
    {
        public static byte[] GetImageBytesFromImagePath(string imagePath)
        {
            if (imagePath == "")
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(imagePath);
            } 
            catch(IOException ex)
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
    }
}
