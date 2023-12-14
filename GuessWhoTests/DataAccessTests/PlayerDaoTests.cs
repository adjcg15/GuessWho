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

        [Fact]
        public void TestGetTopPlayersNegativeLengthFail()
        {
            var players = PlayerDao.GetTopPlayers(string.Empty, fixture.InvalidNumberOfPlayers);

            Assert.True(players.Value.Count == 0);
            Assert.Equal(ResponseStatus.OK, players.StatusCode);
        }

        [Fact]
        public void TestCheckPlayerPermanentBanPlayerBannedSuccess()
        {
            var isPermaBanned = PlayerDao.CheckPlayerPermanentBan(fixture.EmailPermanentlyBannedPlayer);

            Assert.Equal(ResponseStatus.OK, isPermaBanned.StatusCode);
            Assert.True(isPermaBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerPermanentBanPlayerNotBannedSuccess()
        {
            var isPermaBanned = PlayerDao.CheckPlayerPermanentBan(fixture.EmailNotBannedPlayer);

            Assert.Equal(ResponseStatus.OK, isPermaBanned.StatusCode);
            Assert.False(isPermaBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerPermanentBanWithOneReportSuccess()
        {
            var isPermaBanned = PlayerDao.CheckPlayerPermanentBan(fixture.EmailOnePermanentReportPlayer);

            Assert.Equal(ResponseStatus.OK, isPermaBanned.StatusCode);
            Assert.False(isPermaBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerPermanentBanWithTwoReportSuccess()
        {
            var isPermaBanned = PlayerDao.CheckPlayerPermanentBan(fixture.EmailTwoPermanentReportsPlayer);

            Assert.Equal(ResponseStatus.OK, isPermaBanned.StatusCode);
            Assert.False(isPermaBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerPermanentBanEmailNonexistentFail()
        {
            var isInexistentPlayerPermanentlyBanned = PlayerDao.CheckPlayerPermanentBan(fixture.InexistentEmailBannedPlayer);

            Assert.Equal(ResponseStatus.VALIDATION_ERROR, isInexistentPlayerPermanentlyBanned.StatusCode);
            Assert.False(isInexistentPlayerPermanentlyBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerTemporalBanPlayerBannedSuccess()
        {
            var isTemporarilyBanned = PlayerDao.CheckPlayerTemporalBan(fixture.EmailTemporarilyBannedPlayer);

            Assert.Equal(ResponseStatus.NOT_ALLOWED, isTemporarilyBanned.StatusCode);
            Assert.True(isTemporarilyBanned.Value >= DateTime.Now);
        }

        [Fact]
        public void TestCheckPlayerTemporalBanNotBannedSuccess()
        {
            var isTemporarilyBanned = PlayerDao.CheckPlayerTemporalBan(fixture.EmailNotBannedPlayer);

            Assert.Equal(ResponseStatus.OK, isTemporarilyBanned.StatusCode);
            Assert.Equal(DateTime.MinValue, isTemporarilyBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerTemporalBanFinishedBanSuccess()
        {
            var isStillBanned = PlayerDao.CheckPlayerTemporalBan(fixture.EmailPastBannedPlayer);

            Assert.Equal(ResponseStatus.OK, isStillBanned.StatusCode);
            Assert.Equal(DateTime.MinValue, isStillBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerTemporalBanWithOneReportSuccess()
        {
            var isTemporarilyBanned = PlayerDao.CheckPlayerTemporalBan(fixture.EmailOneTemporalReportPlayer);

            Assert.Equal(ResponseStatus.OK, isTemporarilyBanned.StatusCode);
            Assert.Equal(DateTime.MinValue, isTemporarilyBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerTemporalBanWithTwoReportsSuccess()
        {
            var isTemporarilyBanned = PlayerDao.CheckPlayerTemporalBan(fixture.EmailTwoTemporalReportsPlayer);

            Assert.Equal(ResponseStatus.OK, isTemporarilyBanned.StatusCode);
            Assert.Equal(DateTime.MinValue, isTemporarilyBanned.Value);
        }

        [Fact]
        public void TestCheckPlayerTemporalBanEmailNonexistentFail()
        {
            var isInexistentPlayerTemporarilyBanned = PlayerDao.CheckPlayerTemporalBan(fixture.InexistentEmailBannedPlayer);

            Assert.Equal(ResponseStatus.VALIDATION_ERROR, isInexistentPlayerTemporarilyBanned.StatusCode);
            Assert.Equal(DateTime.MinValue, isInexistentPlayerTemporarilyBanned.Value);
        }
    }

    public class PlayerDaoTestsFixture : IDisposable
    {
        public int InvalidNumberOfPlayers { get { return -1; } }
        public int IdWorstPlayerBehaviourReportType { get { return 1; } }
        public string EmailPermanentlyBannedPlayer { get { return "banned-player@test.com"; } } 
        public string EmailNotBannedPlayer { get { return "test-not-bannedplayer@gmail.com"; } }
        public string EmailOnePermanentReportPlayer { get { return "one-permaban-report-player@test.com"; } }
        public string EmailTwoPermanentReportsPlayer { get { return "two-permaban-reports-player@test.com"; } }
        public string InexistentEmailBannedPlayer { get { return "nonexistentEmail@nonexistent.com"; } }
        public int IdRegularPlayerBehaviourReportType { get { return 2; } }
        public string EmailTemporarilyBannedPlayer { get { return "temporarily-banned-player@test.com"; } }
        public string EmailPastBannedPlayer { get { return "past-banned-player@test.com"; } }
        public string EmailOneTemporalReportPlayer { get { return "one-temp-report-player@test.com"; } }
        public string EmailTwoTemporalReportsPlayer { get { return "two-temp-reports-player@test.com"; } }


        public PlayerDaoTestsFixture()
        {
            ServerLogger.ConfigureLogger();

            using (var context = new GuessWhoContext())
            {
                Account accountPermaBan = new Account()
                {
                    email = "banned-player@test.com",
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(accountPermaBan);
                context.SaveChanges();

                User userPermaBan = new User
                {
                    nickname = "00Xs20",
                    fullName = "Sabino López Martínez",
                    idAccount = accountPermaBan.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(userPermaBan);
                context.SaveChanges();

                for (int i = 1; i <= 3; i++)
                {
                    context.Reports.Add(new Report
                    {
                        comment = "Report Comment Explaining situation",
                        idReportedUser = userPermaBan.idUser,
                        idReportType = IdWorstPlayerBehaviourReportType,
                    });
                }
                context.SaveChanges();

                Account accountNotBanned = new Account()
                {
                    email = "test-not-bannedplayer@gmail.com",
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(accountNotBanned);
                context.SaveChanges();

                User userNotBanned = new User()
                {
                    nickname = "NOT-BANNED-PLAYER",
                    fullName = "NOT BANNED PLAYER",
                    idAccount = accountNotBanned.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(userNotBanned);
                context.SaveChanges();

                Account accountOnePermanentReport = new Account
                {
                    email = EmailOnePermanentReportPlayer,
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(accountOnePermanentReport);
                context.SaveChanges();

                User userOnePermanentReport = new User
                {
                    nickname = "OnePermaReport",
                    fullName = "ONE PERMABAN REPORT",
                    idAccount = accountOnePermanentReport.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(userOnePermanentReport);
                context.SaveChanges();

                context.Reports.Add(new Report
                {
                    comment = "1 PermaBan report",
                    idReportedUser = userOnePermanentReport.idUser,
                    idReportType = IdWorstPlayerBehaviourReportType,
                    timestamp = DateTime.Now
                });

                Account accountTwoPermanentReports = new Account
                {
                    email = EmailTwoPermanentReportsPlayer,
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(accountTwoPermanentReports);
                context.SaveChanges();

                User userTwoPermanentReports = new User
                {
                    nickname = "TwoPermaReports",
                    fullName = "TWO PERMABAN REPORTS",
                    idAccount = accountTwoPermanentReports.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(userTwoPermanentReports);
                context.SaveChanges();

                for (int i = 1; i <= 2; i++)
                {
                    context.Reports.Add(new Report
                    {
                        comment = "Permaban reports",
                        idReportedUser = userTwoPermanentReports.idUser,
                        idReportType = IdWorstPlayerBehaviourReportType,
                        timestamp = DateTime.Now
                    });
                }
                context.SaveChanges();

                Account accountTemporarilyBanned = new Account
                {
                    email = EmailTemporarilyBannedPlayer,
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(accountTemporarilyBanned);
                context.SaveChanges();

                User userTemporarilyBanned = new User
                {
                    nickname = "TEMPORARILY",
                    fullName = "TEMPORARILY BANNED PLAYER",
                    idAccount = accountTemporarilyBanned.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(userTemporarilyBanned);
                context.SaveChanges();

                for (int i = 1; i <= 3; i++)
                {
                    context.Reports.Add(new Report
                    {
                        comment = "Report Comment Explaining situation",
                        idReportedUser = userTemporarilyBanned.idUser,
                        idReportType = IdRegularPlayerBehaviourReportType,
                        timestamp = DateTime.Now
                    }); 
                }
                context.SaveChanges();

                Account pastBannedAccount = new Account
                {
                    email = EmailPastBannedPlayer,
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(pastBannedAccount);
                context.SaveChanges();

                User pastBannedUser = new User
                {
                    nickname = "ImFREE",
                    fullName = "Im no Longer banned",
                    idAccount = pastBannedAccount.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(pastBannedUser);
                context.SaveChanges();

                for (int i = 1; i <= 3; i++)
                {
                    context.Reports.Add(new Report
                    {
                        comment = "31 days ago Report",
                        idReportedUser = pastBannedUser.idUser,
                        idReportType = IdRegularPlayerBehaviourReportType,
                        timestamp = DateTime.Now.AddDays(-31)
                    });
                }
                context.SaveChanges();

                Account oneTemporalReportAccount = new Account
                {
                    email = EmailOneTemporalReportPlayer,
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(oneTemporalReportAccount);
                context.SaveChanges();

                User oneTemporalReportUser = new User
                {
                    nickname = "OnlyOneTempReport",
                    fullName = "I just have one temporal report",
                    idAccount = oneTemporalReportAccount.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(oneTemporalReportUser);
                context.SaveChanges();

                context.Reports.Add(new Report
                {
                    comment = "This guy only has one report for out of antisportsmanship",
                    idReportedUser = oneTemporalReportUser.idUser,
                    idReportType = IdRegularPlayerBehaviourReportType,
                    timestamp = DateTime.Now
                });
                context.SaveChanges();

                Account twoTemporalReportsAccount = new Account
                {
                    email = EmailTwoTemporalReportsPlayer,
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(twoTemporalReportsAccount); 
                context.SaveChanges();

                User twoTemporalReportsUser = new User
                {
                    nickname = "TwoTempReports",
                    fullName = "I have two temporal reports",
                    idAccount = twoTemporalReportsAccount.idAccount,
                    avatar = new byte[0]
                };
                context.Users.Add(twoTemporalReportsUser);
                context.SaveChanges();

                for (int i = 1; i <= 2; i++)
                {
                    context.Reports.Add(new Report
                    {
                        comment = "Temporal reports ",
                        idReportedUser = twoTemporalReportsUser.idUser,
                        idReportType = IdRegularPlayerBehaviourReportType,
                        timestamp = DateTime.Now
                    });
                }
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            using (var context = new GuessWhoContext())
            {
                Account accountPermaBannedToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailPermanentlyBannedPlayer);
                context.Accounts.Remove(accountPermaBannedToRemove);

                Account accountNotBannedToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailNotBannedPlayer);
                context.Accounts.Remove(accountNotBannedToRemove);

                Account accountOnePermaReportToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailOnePermanentReportPlayer);
                context.Accounts.Remove(accountOnePermaReportToRemove);

                Account accountTwoPermaReportsToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailTwoPermanentReportsPlayer);
                context.Accounts.Remove(accountTwoPermaReportsToRemove);

                Account accountTemporarilyBannedToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailTemporarilyBannedPlayer);
                context.Accounts.Remove(accountTemporarilyBannedToRemove);

                Account accountPastBannedToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailPastBannedPlayer);
                context.Accounts.Remove(accountPastBannedToRemove);

                Account accountOneTemporalReportToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailOneTemporalReportPlayer);
                context.Accounts.Remove(accountOneTemporalReportToRemove);

                Account accountTwoTemporalReportsToRemove = context.Accounts.FirstOrDefault(a => a.email == EmailTwoTemporalReportsPlayer);
                context.Accounts.Remove(accountTwoTemporalReportsToRemove);

                context.SaveChanges();
            }
        }

    }
}
