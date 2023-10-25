using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoTests.ClientTests.Utils
{
    public class AuthenticationTests
    {
        [Fact]
        public void Authentication_HashPassword_HashCorrectlyGenerated()
        {
            string hashedPassword = Authentication.HashPassword("Juan_killer777");
            string expectedHash = "1356d4e2047f0b91d0b85928c410341ac3bc8882a02e8d91b1ddb3ed92963646";

            Assert.Equal(expectedHash, hashedPassword);
        }

        [Fact]
        public void Authentication_HashPassword_HashNotEqualToPlain()
        {
            string plainPassword = "Juan_killer777";
            string hashedPassword = Authentication.HashPassword(plainPassword);

            Assert.NotEqual(plainPassword, hashedPassword);
        }

        [Fact]
        public void Authentication_IsValidEmail_CorrectEmailIsValid()
        {
            string email = "adjcg15@gmail.com";

            Assert.True(Authentication.IsValidEmail(email));
        }

        [Fact]
        public void Authentication_IsValidEmail_IncorrectEmailIsNotValid()
        {
            string email = "juan-gmail.com";

            Assert.False(Authentication.IsValidEmail(email));
        }

        [Fact]
        public void Authentication_IsValidEmail_EmptyStringIsNotValid()
        {
            string email = "";

            Assert.False(Authentication.IsValidEmail(email));
        }

        [Fact]
        public void Authentication_IsSecurePassword_SecurePasswordIsValid()
        {
            string password = "Jual_killer1";

            Assert.True(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void Authentication_IsSecurePassword_PasswordWithoutEightCharactersIsInsecure()
        {
            string password = "hola";

            Assert.False(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void Authentication_IsSecurePassword_PasswordWithoutNumberIsInsecure()
        {
            string password = "holamundo";

            Assert.False(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void Authentication_IsSecurePassword_PasswordWithoutCapitalLetterIsInsecure()
        {
            string password = "holamundo0";

            Assert.False(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void Authentication_IsSecurePassword_PasswordWithoutSpecialCharacterIsInsecure()
        {
            string password = "Holamundo0";

            Assert.False(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void Authentication_IsSecurePassword_EmptyPasswordInvalid()
        {
            string password = "";

            Assert.False(Authentication.IsSecurePassword(password));
        }

        [Fact]
        public void Authentication_IsValidNickname_ValidNickname()
        {
            string password = "_777JUAN777_";

            Assert.True(Authentication.IsValidNickname(password));
        }

        [Fact]
        public void Authentication_IsValidNickname_NicknameWithSpecialCharacterInvalid()
        {
            string password = "@777JUAN777@";

            Assert.False(Authentication.IsValidNickname(password));
        }

        [Fact]
        public void Authentication_IsValidNickname_EmptyNicknameInvalid()
        {
            string password = "";

            Assert.False(Authentication.IsValidNickname(password));
        }
    }
}
