using GuessWhoDataAccess;
using System;
using System.Linq;
using Xunit;

namespace GuessWhoTests.DataAccessTests
{
    public class ReportDAOTests : IClassFixture<ReportDAOTestsFixture>
    {
        private readonly ReportDAOTestsFixture fixture;

        public ReportDAOTests(ReportDAOTestsFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void TestGetReportsByUserIdSuccess()
        {
            int expectedUserReports = 5;

            var reports = ReportDao.GetReportsByUserId(fixture.IdUserAlreadyRegisteredWithReports);

            Assert.True(reports.Value.Count >= expectedUserReports);
        }

        [Fact]
        public void TestGetReportsByUserIdFail()
        {
            int expectedUserReports = 0;

            var reports = ReportDao.GetReportsByUserId(-1);

            Assert.True(reports.Value.Count == expectedUserReports);
        }

        [Fact]
        public void TestAddPlayerReportSuccess()
        {
            Report newReport = new Report
            {
                comment = "This report will be deleted",
                idReportedUser = fixture.IdUserPreparedToGetReports,
                idReportType = fixture.IdReportTypeAlreadyRegistered
            };

            var reportRegistered = ReportDao.AddPlayerReport(newReport);

            Assert.True(reportRegistered.Value);
        }

        [Fact]
        public void TestAddPlayerReportNullFail()
        {
            Report newReport = null;

            var reportRegistered = ReportDao.AddPlayerReport(newReport);

            Assert.False(reportRegistered.Value);
            Assert.Equal(ResponseStatus.UPDATE_ERROR, reportRegistered.StatusCode);
        }

        [Fact]
        public void TestAddPlayerReportInvalidUserFail()
        {
            Report newReport = new Report
            {
                comment = "This report will be deleted",
                idReportedUser = -1,
                idReportType = fixture.IdReportTypeAlreadyRegistered
            };

            var reportRegistered = ReportDao.AddPlayerReport(newReport);

            Assert.False(reportRegistered.Value);
            Assert.Equal(ResponseStatus.UPDATE_ERROR, reportRegistered.StatusCode);
        }

        [Fact]
        public void TestAddPlayerReportInvalidReportTypeFail()
        {
            Report newReport = new Report
            {
                comment = "This report will be deleted",
                idReportedUser = fixture.IdUserPreparedToGetReports,
                idReportType = -1
            };

            var reportRegistered = ReportDao.AddPlayerReport(newReport);

            Assert.False(reportRegistered.Value);
            Assert.Equal(ResponseStatus.UPDATE_ERROR, reportRegistered.StatusCode);
        }
    }

    public class ReportDAOTestsFixture : IDisposable
    {
        public int IdUserAlreadyRegisteredWithReports { get; }

        public int IdUserPreparedToGetReports { get; }

        public int IdReportTypeAlreadyRegistered { get; }

        private int idAccountAlreadyRegistered;
        private int idAccountPreparedToReports;

        public ReportDAOTestsFixture()
        {
            ServerLogger.ConfigureLogger();

            using (var context = new GuessWhoContext())
            {
                Account newAccount = new Account
                {
                    email = "unit-testing@test.com",
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                Account accountPreparedToGetReports = new Account
                {
                    email = "preparedto@reports.com",
                    password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
                };
                context.Accounts.Add(newAccount);
                context.Accounts.Add(accountPreparedToGetReports);
                context.SaveChanges();

                idAccountAlreadyRegistered = newAccount.idAccount;
                idAccountPreparedToReports = accountPreparedToGetReports.idAccount;

                User newUser = new User
                {
                    nickname = "0juankiller0",
                    fullName = "Juan Manuel Valencia Torres",
                    idAccount = newAccount.idAccount
                };
                User userPreparedToReports = new User
                {
                    nickname = "8educato8",
                    fullName = "Eduardo Cabrera Torres",
                    idAccount = accountPreparedToGetReports.idAccount
                };
                context.Users.Add(newUser);
                context.Users.Add(userPreparedToReports);
                context.SaveChanges();

                IdUserAlreadyRegisteredWithReports = newUser.idUser;
                IdUserPreparedToGetReports = userPreparedToReports.idUser;

                ReportType newReportType = context.ReportTypes.Add(new ReportType
                {
                    name = "ExampleReportType"
                });
                context.SaveChanges();
                IdReportTypeAlreadyRegistered = newReportType.idReportType;

                for (int i = 1; i <= 5; i++)
                {
                    context.Reports.Add(new Report
                    {
                        idReportedUser = IdUserAlreadyRegisteredWithReports,
                        comment = "Report " + i,
                        idReportType = IdReportTypeAlreadyRegistered
                    });
                }
                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            using (var context = new GuessWhoContext())
            {
                Account accountToDelete = context.Accounts.FirstOrDefault(a => a.idAccount == idAccountAlreadyRegistered);
                context.Accounts.Remove(accountToDelete);

                Account accountThatReceivedReports = context.Accounts.FirstOrDefault(a => a.idAccount == idAccountPreparedToReports);
                context.Accounts.Remove(accountThatReceivedReports);

                ReportType reportTypeToDelete = context.ReportTypes.FirstOrDefault(rt => rt.name == "ExampleReportType");
                context.ReportTypes.Remove(reportTypeToDelete);

                context.SaveChanges();
            }
        }
    }
}