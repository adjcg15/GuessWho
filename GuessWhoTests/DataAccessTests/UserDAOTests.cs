using GuessWhoDataAccess;
using System;
using System.Linq;
using Xunit;

namespace GuessWhoTests.DataAccessTests
{
    public class UserDAOTests: IDisposable
    {
		private const string TEST_ACCOUNT_EMAIL = "unit-testing@test.com";
		private const string TEST_ACCOUNT_PASSWORD_HASH = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79";

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

            var account = UserDao.VerifyUserSession(email, hashedPassword);
            
            Assert.NotNull(account);
		}

		[Fact]
		public void TestVerifyUserSessionFail()
		{
			string email = "not-registered-email@gmail.com";
			string hashedPassword = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79";

			var account = UserDao.VerifyUserSession(email, hashedPassword);

			Assert.Null(account);
		}
    }
}
