using GuessWhoDataAccess;
using System;
using System.Linq;
using Xunit;

namespace GuessWhoTests.DataAccessTests
{
    public class MatchDAOTests : IClassFixture<MatchDAOTestsFixture>
    {
        private MatchDAOTestsFixture fixture;

        public MatchDAOTests(MatchDAOTestsFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestAddScorePointsSuccess()
        {
            int expectedScore = 5;

            MatchDao.AddScorePoints(fixture.NicknameUserAlreadyRegistered);

            using (var context = new GuessWhoContext())
            {
                int? scoreStored = context.Matches
                    .Where(match => match.idWinner == fixture.IdUserAlreadyRegistered)
                    .Sum(match => match.score);

                Assert.Equal(expectedScore, scoreStored);
            }
        }

        [Fact]
        public void TestAddScorePointsWinnerNicknameEmptyFail()
        {
            var response = MatchDao.AddScorePoints(string.Empty);

            Assert.NotEqual(ResponseStatus.OK, response.StatusCode);
            Assert.False(response.Value);
        }

        [Fact]
        public void TestAddScorePointsWinnerNicknameInvalidFail()
        {
            var response = MatchDao.AddScorePoints(fixture.InexistentWinnerNickname);

            Assert.NotEqual(ResponseStatus.OK, response.StatusCode);
            Assert.False(response.Value);
        }
    }

    public class MatchDAOTestsFixture : IDisposable
    {
        public string NicknameUserAlreadyRegistered { get { return "0juankiller0"; } }
        public int IdUserAlreadyRegistered { get; }

        private int idAccountAlreadyRegistered;

        public string InexistentWinnerNickname { get { return "INEXISTENT_NICKNAME"; } }
        public MatchDAOTestsFixture()
        {
            ServerLogger.ConfigureLogger();

            using (var context = new GuessWhoContext())
            {
                Account newAccount = new Account
                {
                    email = "unit-testing@test.com",
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(newAccount);
                context.SaveChanges();
                idAccountAlreadyRegistered = newAccount.idAccount;

                User newUser = new User
                {
                    nickname = NicknameUserAlreadyRegistered,
                    fullName = "Juan Manuel Valencia Torres",
                    idAccount = newAccount.idAccount
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
                Account accountToDelete = context.Accounts.FirstOrDefault(a => a.idAccount == idAccountAlreadyRegistered);
                context.Accounts.Remove(accountToDelete);

                context.SaveChanges();
            }
        }
    }
}
