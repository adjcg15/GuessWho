using GuessWhoDataAccess;
using System;
using System.Linq;
using Xunit;

namespace GuessWhoTests.DataAccessTests
{
    public class UserDaoTests: IClassFixture<UserDaoTestsFixture>
    {
        private readonly UserDaoTestsFixture fixture;

        public UserDaoTests(UserDaoTestsFixture fixture)
        {
            this.fixture = fixture;
        }

		[Fact]
        public void TestVerifyUserSessionSuccess()
        {
            var account = UserDao.VerifyUserSession(fixture.EmailAccountAlreadyRegistered, fixture.PasswordAccountAlreadyRegistered);
            
            Assert.NotNull(account.Value);
		}

		[Fact]
		public void TestVerifyUserSessionFail()
		{
			string email = "not-registered-email@gmail.com";
			string hashedPassword = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79";

			var account = UserDao.VerifyUserSession(email, hashedPassword);

			Assert.Null(account.Value);
		}

        [Fact]
        public void TestRegisterUserSuccess()
        {
            User newUser = new User
            {
                nickname = "sofwinner890",
                fullName = "Sofía Sánchez López"
            };
            Account newAccount = new Account
            {
                email = fixture.EmailNewAccountRegistered,
                password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
            };

            var response = UserDao.RegisterUser(newUser, newAccount);

            Assert.Equal(ResponseStatus.OK, response.StatusCode);
            Assert.True(response.Value);
        }

        [Fact]
        public void TestGetUserByIdAccountSuccess()
        {
            var response = UserDao.GetUserByIdAccount(fixture.IdAccountAlreadyRegistered);

            Assert.Equal(ResponseStatus.OK, response.StatusCode);
            Assert.NotNull(response.Value);
        }

        [Fact]
        public void TestGetUserByIdAccountFail()
        {
            const int INVALID_ID_ACCOUNT = -1;

            var response = UserDao.GetUserByIdAccount(INVALID_ID_ACCOUNT);

            Assert.Null(response.Value);
            Assert.Equal(ResponseStatus.OK, response.StatusCode);
        }

        [Fact]
        public void TestGetUserByEmailSuccess()
        {
            var response = UserDao.GetUserByEmail(fixture.EmailAccountAlreadyRegistered);

            Assert.NotNull(response.Value);
        }

        [Fact]
        public void TestGetUserByEmailFail()
        {
            string nonExistentEmail = "not-registered-email@gmail.com";

            var response = UserDao.GetUserByEmail(nonExistentEmail);

            Assert.Null(response.Value);
        }

        [Fact]
        public void TestGetUserByNicknameSuccess()
        {
            var response = UserDao.GetUserByNickName(fixture.NicknameUserAlreadyRegistered);

            Assert.NotNull(response.Value);
        }

        [Fact]
        public void TestGetUserByNicknameFail()
        {
            var response = UserDao.GetUserByNickName("balloonJeans597");

            Assert.Null(response.Value);
        }

        [Fact]
        public void TestUpdateUserProfileImageSuccess()
        {
            var response = UserDao.UpdateUserProfileImage(new byte[0], fixture.IdUserAlreadyRegistered);

            Assert.True(response.Value);
        }

        [Fact]
        public void TestUpdateUserProfileImageFail()
        {
            var response = UserDao.UpdateUserProfileImage(new byte[0], -1);

            Assert.False(response.Value);
        }

        [Fact]
        public void TestGetLastTimeNicknameChangeByIdSuccess()
        {
            DateTime expectedLastUpdate = new DateTime(2001, 04, 15);

            var lastUpdateDate = UserDao.GetLastTimeNicknameChangeById(fixture.IdUserAlreadyRegistered);

            Assert.Equal(lastUpdateDate.Value, expectedLastUpdate);
        }

        [Fact]
        public void TestUpdateUserNicknameSuccess()
        {
            string newNickname = "555pedrito555";

            UserDao.UpdateUserNickname(newNickname, fixture.IdUserAlreadyRegistered);
            using (var context = new GuessWhoContext())
            {
                User userUpdated = context.Users.FirstOrDefault(a => a.idUser == fixture.IdUserAlreadyRegistered);
                Assert.Equal(userUpdated.nickname, newNickname);

                UserDao.UpdateUserNickname(fixture.NicknameUserAlreadyRegistered, fixture.IdUserAlreadyRegistered);
            }
        }

        [Fact]
        public void TestUpdateUserNicknameFail()
        {
            string newNickname = "555pedrito555";

            var updateResult = UserDao.UpdateUserNickname(newNickname, -1);
            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestUpdateUserFullnameSuccess()
        {
            string newName = "Paola Martínez Navarro";

            var updateResult = UserDao.UpdateUserFullname(newName, fixture.IdUserAlreadyRegistered);
            using (var context = new GuessWhoContext())
            {
                User userUpdated = context.Users.FirstOrDefault(a => a.idUser == fixture.IdUserAlreadyRegistered);
                Assert.Equal(userUpdated.fullName, newName);
            }
        }

        [Fact]
        public void TestUpdateUserFullnameFail()
        {
            string newName = "Paola Martínez Navarro";

            var updateResult = UserDao.UpdateUserFullname(newName, -1);
            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestUpdateAccountPasswordSuccess()
        {
            string newPassword = "b21b0777c65acdf92b7c279595c43cfe475fbb87722286eb94982be1f05f0c31";

            var updateResult = UserDao.UpdateAccountPassword(newPassword, fixture.IdAccountAlreadyRegistered);
            using (var context = new GuessWhoContext())
            {
                Account accountUpdated = context.Accounts.FirstOrDefault(a => a.idAccount == fixture.IdAccountAlreadyRegistered);
                Assert.Equal(accountUpdated.password, newPassword);

                UserDao.UpdateAccountPassword("b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79", fixture.IdAccountAlreadyRegistered);
            }
        }

        [Fact]
        public void TestUpdateAccountPasswordFail()
        {
            string newPassword = "b21b0777c65acdf92b7c279595c43cfe475fbb87722286eb94982be1f05f0c31";

            var updateResult = UserDao.UpdateAccountPassword(newPassword, -1);
            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestGetUserAvatarByNicknameSuccess()
        {
            var avatar = UserDao.GetUserAvatarByNickname(fixture.NicknameUserAlreadyRegistered);
            Assert.NotNull(avatar.Value);
        }
    }

    public class UserDaoTestsFixture : IDisposable
    {
        public string EmailAccountAlreadyRegistered { get { return "unit-testing@test.com"; } }
        public string NicknameUserAlreadyRegistered { get { return "0juankiller0"; } }
        public int IdAccountAlreadyRegistered { get; }
        public int IdUserAlreadyRegistered { get; }
        public string PasswordAccountAlreadyRegistered { get { return "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"; } }
        public string EmailNewAccountRegistered { get { return "sofia@gmail.com"; } }

        public UserDaoTestsFixture()
        {
            using (var context = new GuessWhoContext())
            {
                Account newAccount = new Account
                {
                    email = EmailAccountAlreadyRegistered,
                    password = PasswordAccountAlreadyRegistered
                };
                context.Accounts.Add(newAccount);
                context.SaveChanges();

                IdAccountAlreadyRegistered = newAccount.idAccount;
                User newUser = new User
                {
                    nickname = NicknameUserAlreadyRegistered,
                    fullName = "Juan Manuel Valencia Torres",
                    idAccount = newAccount.idAccount,
                    lastTimeNicknameChanged = new DateTime(2001, 04, 15),
                    avatar = new byte[0]
                };
                context.Users.Add(newUser);
                context.SaveChanges();

                IdUserAlreadyRegistered = newUser.idUser;
            }
        }

        public void Dispose()
        {
            using (var context = new GuessWhoContext())
            {
                Account accountToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailAccountAlreadyRegistered);
                context.Accounts.Remove(accountToRemove);

                Account newAccount = context.Accounts.FirstOrDefault(a => a.email == EmailNewAccountRegistered);
                context.Accounts.Remove(newAccount);

                context.SaveChanges();
            }
        }
    }
}
