using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Xunit;

namespace GuessWhoTests.ClientTests.Utils
{
    public class ImageTransformatorTests
    {
        [Fact]
        public void TestGetImageBytesFromImagePathSuccess()
        {
            string testImagePath = Path.Combine("..", "..", "..", "GuessWhoClient", "Resources", "logo.ES-MX.png");

            string fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testImagePath));

            byte[] result = ImageTransformator.GetImageBytesFromImagePath(fullPath);

            Assert.NotEmpty(result);
        }

        [Fact]
        public void TestGetImageBytesFromImagePathFail()
        {
            string invalidImagePath = "path_to_nonexistent_image.jpg";

            byte[] result = ImageTransformator.GetImageBytesFromImagePath(invalidImagePath);

            Assert.Empty(result);
        }

        [Fact]
        public void TestGetImageBytesFromImageEmptyPathFail()
        {
            string invalidImagePath = string.Empty;

            byte[] result = ImageTransformator.GetImageBytesFromImagePath(invalidImagePath);

            Assert.Empty(result);
        }

        [Fact]
        public void TestGetBitmapImageFromByteArraySuccess()
        {
            string testImagePath = Path.Combine("..", "..", "..", "GuessWhoClient", "Resources", "logo.ES-MX.png");

            string fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testImagePath));

            byte[] imageBytes = ImageTransformator.GetImageBytesFromImagePath(fullPath);

            BitmapImage result = ImageTransformator.GetBitmapImageFromByteArray(imageBytes); 
            Assert.NotNull(result);
        }

        [Fact]
        public void TestGetBitmapImageFromByteArrayEmptyFail()
        {
            byte[] emptyImageBytes = new byte[0];

            BitmapImage result = ImageTransformator.GetBitmapImageFromByteArray(emptyImageBytes);
            Assert.Null(result);
        }

        [Fact]
        public void TestGetBitmapImageFromByteArrayNullFail()
        {
            byte[] nullImageBytes = null;

            BitmapImage result = ImageTransformator.GetBitmapImageFromByteArray(nullImageBytes);
            Assert.Null(result);
        }
    }
}
