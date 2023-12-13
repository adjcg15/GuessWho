using GuessWhoDataAccess;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GuessWhoTests.DataAccessTests
{
    public class PlayerDaoTests: IClassFixture<PlayerDaoTestsFixture>
    {
        private readonly PlayerDaoTestsFixture fixture;

        public PlayerDaoTests(PlayerDaoTestsFixture fixture) 
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestGetTopPlayersSucces()
        {
            var response = PlayerDao.GetTopPlayers(fixture.SearchQuery, fixture.ValidNumberOfTopPlayers);

            Assert.Equal(ResponseStatus.OK, response.StatusCode);
        }

        [Fact]
        public void TestGetTopPlayersFail()
        {
            var response = PlayerDao.GetTopPlayers(fixture.SearchQuery, fixture.InvalidNumberOfTopPlayers);

            Assert.Equal(ResponseStatus.OK, response.StatusCode);
        }
    }

    public class PlayerDaoTestsFixture : IDisposable
    {
        public string EmailAccountRegistered { get { return "unit-testing@test.com"; } }
        public string PasswordAccountRegistered { get { return "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"; } }
        public int IdUserRegistered { get; }
        public string SearchQuery { get { return ""; } }
        public int ValidNumberOfTopPlayers { get { return 100; } }
        public int InvalidNumberOfTopPlayers { get { return -1; } }

        public PlayerDaoTestsFixture()
        {
            using (var context = new GuessWhoContext())
            {
                Account newAccount = new Account()
                {
                    email = EmailAccountRegistered,
                    password = PasswordAccountRegistered
                };
                context.Accounts.Add(newAccount);
                context.SaveChanges();

                User newUser = new User
                {
                    nickname = "00Xs20",
                    fullName = "Sabino López Martínez",
                    idAccount = newAccount.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(newUser);
                context.SaveChanges();

                IdUserRegistered = newUser.idUser;
            }
        }

        public void Dispose()
        {
            using (var context = new GuessWhoContext())
            {
                Account accountToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailAccountRegistered);
                context.Accounts.Remove(accountToRemove);

                context.SaveChanges();
            }
        }  
          
    }
}
