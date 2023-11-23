using GuessWhoClient.Utils;
using Xunit;

namespace GuessWhoTests.ClientTests.Utils
{
    public class AuthenticationTests
    {
        [Fact]
        public void TestHashPasswordSuccess()
        {
            string hashedPassword = Authentication.HashPassword("Juan_killer777");
            string expectedHash = "1356d4e2047f0b91d0b85928c410341ac3bc8882a02e8d91b1ddb3ed92963646";

            Assert.Equal(expectedHash, hashedPassword);
        }

        [Fact]
        public void TestIsValidEmailSuccess()
        {
            string email = "adjcg15@gmail.com";

            Assert.True(Authentication.IsValidEmail(email));
        }

        [Fact]
        public void TestIsValidEmailEmptyFail()
        {
            string email = "";

            Assert.False(Authentication.IsValidEmail(email));
        }

        [Fact]
        public void TestIsValidEmailFail()
        {
            string email = "juan-gmail.com";

            Assert.False(Authentication.IsValidEmail(email));
        }

        [Fact]
        public void TestIsSecurePasswordSuccess()
        {
            string password = "Jual_killer1";

            Assert.True(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void TestIsSecurePasswordFail()
        {
            string password = "holamundoooooo";

            Assert.False(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void TestIsValidNicknameSuccess()
        {
            string password = "_777JUAN777_";

            Assert.True(Authentication.IsValidNickname(password));
        }

        [Fact]
        public void TestIsValidNicknameEmptyFail()
        {
            string password = "";

            Assert.False(Authentication.IsValidNickname(password));
        }

        [Fact]
        public void TestIsValidNicknameFail()
        {
            string password = "@[juan]@";

            Assert.False(Authentication.IsValidNickname(password));
        }
    }
}