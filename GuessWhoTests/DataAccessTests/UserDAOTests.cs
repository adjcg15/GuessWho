using GuessWhoDataAccess;
using Moq;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace GuessWhoTests.DataAccessTests
{
    public class UserDAOTests: IDisposable
    {
		private const string TEST_ACCOUNT_EMAIL = "unit-testing@test.com";
		private const string TEST_ACCOUNT_PASSWORD_HASH = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79";
        private const string TEST_USER_NICKNAME = "TESTING";
        private const string TEST_USER_FULLNAME = "TEST";
        private Account globalTestAccount = new Account();

		public UserDAOTests() 
		{
			using (var context = new GuessWhoContext())
            {
				Account testAccount = new Account
				{
					email = TEST_ACCOUNT_EMAIL,
					password = TEST_ACCOUNT_PASSWORD_HASH
				};
				context.Accounts.Add(testAccount);
                context.SaveChanges();

                globalTestAccount = testAccount;
			}
		}

		public void Dispose()
		{
			using (var context = new GuessWhoContext())
			{
				Account testAccount = context.Accounts.FirstOrDefault(a => a.email == TEST_ACCOUNT_EMAIL);
				context.Accounts.Remove(testAccount);
				context.SaveChanges();
            }
		}

		[Fact]
        public void TestVerifyUserSessionSuccess()
        {
            string email = TEST_ACCOUNT_EMAIL;
            string hashedPassword = TEST_ACCOUNT_PASSWORD_HASH;

            var account = UserDAO.VerifyUserSession(email, hashedPassword);
            
            Assert.NotNull(account);
		}

		[Fact]
		public void TestVerifyUserSessionFail()
		{
			string email = "not-registered-email@gmail.com";
			string hashedPassword = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79";

			var account = UserDAO.VerifyUserSession(email, hashedPassword);

			Assert.Null(account.Value);
		}

        [Fact]
        public void TestRegisterUserSuccess()
        {
            User testUser = new User
            {
                fullName = TEST_USER_FULLNAME,
                nickname = TEST_USER_NICKNAME
            };

            Account testAccount = new Account
            {
                email = TEST_ACCOUNT_EMAIL,
                password = TEST_ACCOUNT_PASSWORD_HASH
            };

            var response = UserDAO.RegisterUser(testUser, testAccount);

            Assert.Equal(ResponseStatus.OK, response.StatusCode);
            Assert.True(response.Value);

            using (var context = new GuessWhoContext())
            {
                User userToRemove = context.Users.FirstOrDefault(u => u.idAccount == testAccount.idAccount);
                context.Users.Remove(userToRemove);

                Account accountToRemove = context.Accounts.FirstOrDefault(a => a.idAccount == testAccount.idAccount);
                context.Accounts.Remove(accountToRemove);

                context.SaveChanges();
            }
        }

        //[Fact]
        //public void TestRegisterUserFail()
        //{
        //    var response = UserDAO.RegisterUser(null, null);

        //    Assert.False(response.Value);
        //    Assert.NotEqual(ResponseStatus.OK, response.StatusCode);
        //}

        [Fact]
        public void TestGetUserByIdAccountSuccess()
        {
            var response = UserDAO.GetUserByIdAccount(globalTestAccount.idAccount);

            Assert.Equal(ResponseStatus.OK, response.StatusCode);
            Assert.NotNull(response.Value);
        }

        [Fact]
        public void TestGetUserByIdAccountFail()
        {
            const int INVALID_ID_ACCOUNT = -1; 

            var response = UserDAO.GetUserByIdAccount(INVALID_ID_ACCOUNT);

            Assert.Null(response.Value);
            Assert.Equal(ResponseStatus.OK, response.StatusCode);
        }

        [Fact]
        public void TestGetUserByEmailSuccess()
        {
            var response = UserDAO.GetUserByEmail(TEST_ACCOUNT_EMAIL);

            Assert.NotNull(response.Value);
        }
    }
}
