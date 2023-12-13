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
    public class PlayerDaoTests : IClassFixture<PlayerDaoTestsFixture>
    {
        private readonly PlayerDaoTestsFixture fixture;

        public PlayerDaoTests(PlayerDaoTestsFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestGetTopPlayersSucces()
        {
            var players = PlayerDao.GetTopPlayers("", 10);

            Assert.Equal(ResponseStatus.OK, players.StatusCode);
        }

        [Fact]
        public void TestGetTopPlayersExactNumberSuccess()
        {
            int expectedPlayers = 0;

            var players = PlayerDao.GetTopPlayers("", 10);

            Assert.True(players.Value.Count == expectedPlayers);
        }
    }

    public class PlayerDaoTestsFixture : IDisposable
    {
        private int idAccountAlreadyRegistered;

        public PlayerDaoTestsFixture()
        {
            using (var context = new GuessWhoContext())
            {
                Account newAccount = new Account()
                {
                    email = "unit-testing@test.com",
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(newAccount);
                context.SaveChanges();
                idAccountAlreadyRegistered = newAccount.idAccount;

                User newUser = new User
                {
                    nickname = "00Xs20",
                    fullName = "Sabino López Martínez",
                    idAccount = newAccount.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(newUser);
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            using (var context = new GuessWhoContext())
            {
                Account accountToRemove = context.Accounts.FirstOrDefault(a => a.idAccount == idAccountAlreadyRegistered);
                context.Accounts.Remove(accountToRemove);

                context.SaveChanges();
            }
        }

    }
}
