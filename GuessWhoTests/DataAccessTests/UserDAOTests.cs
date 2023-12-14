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
        public void TestVerifyUserSessionIncorrectPasswordFail()
        {
            string incorrectPassword = "b21b0777c65acdf92b7c279595c43cfe475fbb87722286eb94982be1f05f0c31";

            var account = UserDao.VerifyUserSession(fixture.EmailAccountAlreadyRegistered, incorrectPassword);

            Assert.Equal(ResponseStatus.VALIDATION_ERROR, account.StatusCode);
            Assert.Null(account.Value);
        }

        [Fact]
        public void TestVerifyUserSessionIncorrectEmailFail()
        {
            string incorrectEmail = "unit-testing@tet.com";

            var account = UserDao.VerifyUserSession(incorrectEmail, fixture.PasswordAccountAlreadyRegistered);

            Assert.Equal(ResponseStatus.VALIDATION_ERROR, account.StatusCode);
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
        public void TestRegisterUserNullAccountFail()
        {
            User newUser = new User
            {
                nickname = "sofwinner890",
                fullName = "Sofía Sánchez López"
            };
            Account newAccount = null;

            var accountRegistered = UserDao.RegisterUser(newUser, newAccount);

            Assert.Equal(ResponseStatus.NOT_ALLOWED, accountRegistered.StatusCode);
            Assert.False(accountRegistered.Value);
        }

        [Fact]
        public void TestRegisterUserNullUserFail()
        {
            User newUser = null;
            Account newAccount = new Account
            {
                email = fixture.EmailNewAccountRegistered,
                password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
            }; ;

            var accountRegistered = UserDao.RegisterUser(newUser, newAccount);

            Assert.Equal(ResponseStatus.NOT_ALLOWED, accountRegistered.StatusCode);
            Assert.False(accountRegistered.Value);
        }

        [Fact]
        public void TestRegisterUserEmailInUseFail()
        {
            User newUser = new User
            {
                nickname = "juan8129304921",
                fullName = "Juan Macip Contreras"
            };
            Account newAccount = new Account
            {
                email = fixture.EmailAccountAlreadyRegistered,
                password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
            };

            var response = UserDao.RegisterUser(newUser, newAccount);

            Assert.Equal(ResponseStatus.NOT_ALLOWED, response.StatusCode);
            Assert.False(response.Value);
        }

        [Fact]
        public void TestRegisterUserNicknameInUseFail()
        {
            User newUser = new User
            {
                nickname = fixture.NicknameUserAlreadyRegistered,
                fullName = "Juan Macip Contreras"
            };
            Account newAccount = new Account
            {
                email = "jmcont98@gmail.com",
                password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
            };

            var response = UserDao.RegisterUser(newUser, newAccount);

            Assert.Equal(ResponseStatus.NOT_ALLOWED, response.StatusCode);
            Assert.False(response.Value);
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
            string nonExistentEmail = "not-email@gmail.com";

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
            var response = UserDao.UpdateUserProfileImage(new byte[0], fixture.IdUserPreparedToUpdate);

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

            Assert.Equal(expectedLastUpdate, lastUpdateDate.Value);
        }

        [Fact]
        public void TestGetLastTimeNicknameChangeByIdFail()
        {
            DateTime expectedLastUpdate = DateTime.MaxValue;

            var lastUpdateDate = UserDao.GetLastTimeNicknameChangeById(-1);

            Assert.Equal(expectedLastUpdate, lastUpdateDate.Value);
        }

        [Fact]
        public void TestUpdateUserNicknameSuccess()
        {
            string newNickname = "555pedrito555";

            UserDao.UpdateUserNickname(newNickname, fixture.IdUserPreparedToUpdate);
            using (var context = new GuessWhoContext())
            {
                User userUpdated = context.Users.FirstOrDefault(a => a.idUser == fixture.IdUserPreparedToUpdate);
                Assert.Equal(newNickname, userUpdated.nickname);
            }
        }

        [Fact]
        public void TestUpdateUserNicknameUserNotFoundFail()
        {
            string newNickname = "555pedrito555";

            var updateResult = UserDao.UpdateUserNickname(newNickname, -1);

            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestUpdateUserNicknameEmptyUpdateFail()
        {
            string newNickname = "";

            var updateResult = UserDao.UpdateUserNickname(newNickname, fixture.IdUserPreparedToUpdate);

            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestUpdateUserFullnameSuccess()
        {
            string newName = "Paola Martínez Navarro";

            var updateResult = UserDao.UpdateUserFullname(newName, fixture.IdUserPreparedToUpdate);
            using (var context = new GuessWhoContext())
            {
                User userUpdated = context.Users.FirstOrDefault(a => a.idUser == fixture.IdUserPreparedToUpdate);
                Assert.Equal(newName, userUpdated.fullName);
            }
        }

        [Fact]
        public void TestUpdateUserFullnameUserNotFoundFail()
        {
            string newName = "Paola Martínez Navarro";

            var updateResult = UserDao.UpdateUserFullname(newName, -1);
            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestUpdateUserFullnameEmptyUpdateFail()
        {
            string newName = "";

            var updateResult = UserDao.UpdateUserFullname(newName, fixture.IdUserPreparedToUpdate);
            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestUpdateAccountPasswordSuccess()
        {
            string newPassword = "b21b0777c65acdf92b7c279595c43cfe475fbb87722286eb94982be1f05f0c31";

            var updateResult = UserDao.UpdateAccountPassword(newPassword, fixture.IdAccountPreparedToUpdate);
            using (var context = new GuessWhoContext())
            {
                Account accountUpdated = context.Accounts.FirstOrDefault(a => a.idAccount == fixture.IdAccountPreparedToUpdate);
                Assert.Equal(newPassword, accountUpdated.password);
            }
        }

        [Fact]
        public void TestUpdateAccountPasswordUserNotFoundFail()
        {
            string newPassword = "b21b0777c65acdf92b7c279595c43cfe475fbb87722286eb94982be1f05f0c31";

            var updateResult = UserDao.UpdateAccountPassword(newPassword, -1);
            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestUpdateAccountPasswordEmptyUpdateFail()
        {
            string newPassword = "";

            var updateResult = UserDao.UpdateAccountPassword(newPassword, fixture.IdAccountPreparedToUpdate);
            Assert.False(updateResult.Value);
        }

        [Fact]
        public void TestGetUserAvatarByNicknameSuccess()
        {
            var avatar = UserDao.GetUserAvatarByNickname(fixture.NicknameUserAlreadyRegistered);
            Assert.NotNull(avatar.Value);
        }

        [Fact]
        public void TestGetUserAvatarByNicknameFail()
        {
            string userNickname = "";

            var avatar = UserDao.GetUserAvatarByNickname(userNickname);

            Assert.Null(avatar.Value);
        }
    }

    public class UserDaoTestsFixture : IDisposable
    {
        public string EmailAccountAlreadyRegistered { get { return "unit-testing@test.com"; } }
        public string EmailAccountPreparedToUpdate { get { return "preparedto@update.com"; } }
        public string NicknameUserAlreadyRegistered { get { return "0juankiller0"; } }
        public int IdAccountAlreadyRegistered { get; }
        public int IdAccountPreparedToUpdate { get; }
        public int IdUserAlreadyRegistered { get; }
        public int IdUserPreparedToUpdate { get; }
        public string PasswordAccountAlreadyRegistered { get { return "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"; } }
        public string EmailNewAccountRegistered { get { return "sofia@gmail.com"; } }

        public UserDaoTestsFixture()
        {
            ServerLogger.ConfigureLogger();

            using (var context = new GuessWhoContext())
            {
                Account newAccount = new Account
                {
                    email = EmailAccountAlreadyRegistered,
                    password = PasswordAccountAlreadyRegistered
                };
                Account newAccountPreparedToUpdate = new Account
                {
                    email = EmailAccountPreparedToUpdate,
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(newAccount);
                context.Accounts.Add(newAccountPreparedToUpdate);
                context.SaveChanges();

                IdAccountAlreadyRegistered = newAccount.idAccount;
                IdAccountPreparedToUpdate = newAccountPreparedToUpdate.idAccount;

                User newUser = new User
                {
                    nickname = NicknameUserAlreadyRegistered,
                    fullName = "Juan Manuel Valencia Torres",
                    idAccount = newAccount.idAccount,
                    lastTimeNicknameChanged = new DateTime(2001, 04, 15),
                    avatar = new byte[10]
                };
                User newUserPreparedToUpdate = new User
                {
                    nickname = "cato888",
                    fullName = "Carlos Torres",
                    idAccount = newAccountPreparedToUpdate.idAccount
                };
                context.Users.Add(newUser);
                context.Users.Add(newUserPreparedToUpdate);
                context.SaveChanges();

                IdUserAlreadyRegistered = newUser.idUser;
                IdUserPreparedToUpdate = newUserPreparedToUpdate.idUser;
            }
        }

        public void Dispose()
        {
            using (var context = new GuessWhoContext())
            {
                Account accountToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailAccountAlreadyRegistered);
                context.Accounts.Remove(accountToRemove);

                Account accountWithUpdate = context.Accounts.FirstOrDefault(a => a.email == EmailAccountPreparedToUpdate);
                context.Accounts.Remove(accountWithUpdate);

                Account newAccount = context.Accounts.FirstOrDefault(a => a.email == EmailNewAccountRegistered);
                context.Accounts.Remove(newAccount);

                context.SaveChanges();
            }
        }
    }
}
