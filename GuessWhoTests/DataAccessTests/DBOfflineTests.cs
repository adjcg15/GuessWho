using GuessWhoClient;
using GuessWhoDataAccess;
using System.Runtime.Remoting.Contexts;
using Xunit;

namespace GuessWhoTests.DataAccessTests
{
    public class DBOfflineTests : IClassFixture<DBOfflineTestsFixture>
    {
        [Fact] 
        public void TestVerifyUserSessionFail() 
        {
            string email = "adjcg15@gmail.com";
            string hashedPassword = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79";

            var verification = UserDao.VerifyUserSession(email, hashedPassword);

            Assert.Equal(ResponseStatus.SQL_ERROR, verification.StatusCode);
        }

        [Fact]
        public void TestRegisterUserFail()
        {
            Account account = new Account
            {
                email = "test@gmail.com",
                password = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79"
            };
            User user = new User
            {
                nickname = "testpro123",
                fullName = "Miguel Ángel Cuellar Barragán"
            };

            var userRegistered = UserDao.RegisterUser(user, account);

            Assert.Equal(ResponseStatus.SQL_ERROR, userRegistered.StatusCode);
        }

        [Fact]
        public void TestGetUserByIdAccountFail()
        {
            int idUser = 1;

            var userRegistered = UserDao.GetUserByIdAccount(idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, userRegistered.StatusCode);
        }

        [Fact]
        public void TestGetUserByEmailFail()
        {
            string userEmail = "adjcg15@gmail.com";

            var userRegistered = UserDao.GetUserByEmail(userEmail);

            Assert.Equal(ResponseStatus.SQL_ERROR, userRegistered.StatusCode);
        }

        [Fact]
        public void TestGetUserByNickNameFail()
        {
            string userNickname = "adjcg15";

            var userRegistered = UserDao.GetUserByNickName(userNickname);

            Assert.Equal(ResponseStatus.SQL_ERROR, userRegistered.StatusCode);
        }

        [Fact]
        public void TestUpdateUserProfileImageFail()
        {
            byte[] newAvatar = new byte[10];
            int idUser = 1;

            var updateResult = UserDao.UpdateUserProfileImage(newAvatar, idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, updateResult.StatusCode);
        }

        [Fact]
        public void TestGetLastTimeNicknameChangeByIdFail()
        {
            int idUser = 1;

            var dateUpdated = UserDao.GetLastTimeNicknameChangeById(idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, dateUpdated.StatusCode);
        }

        [Fact]
        public void TestUpdateUserNicknameFail()
        {
            int idUser = 1;
            string newNickname = "777themostpro777";

            var updateResult = UserDao.UpdateUserNickname(newNickname, idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, updateResult.StatusCode);
        }

        [Fact]
        public void TestUpdateUserFullnameFail()
        {
            int idUser = 1;
            string newName = "Juan";

            var updateResult = UserDao.UpdateUserFullname(newName, idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, updateResult.StatusCode);
        }

        [Fact]
        public void TestUpdateAccountPasswordFail()
        {
            int idUser = 1;
            string newPassword = "b221d9dbb083a7f33428d7c2a3c3198ae925614d70210e28716ccaa7cd4ddb79";

            var updateResult = UserDao.UpdateAccountPassword(newPassword, idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, updateResult.StatusCode);
        }

        [Fact]
        public void TestGetUserAvatarByNicknameFail()
        {
            string userNickname = "adjcg15";

            var avatarRegistered = UserDao.GetUserAvatarByNickname(userNickname);

            Assert.Equal(ResponseStatus.SQL_ERROR, avatarRegistered.StatusCode);
        }

        [Fact]
        public void TestAddRequestFail()
        {
            int idUserRequester = 1;
            int idUserRequested = 2;

            var requestResult = FriendshipDao.AddRequest(idUserRequester, idUserRequested);

            Assert.Equal(ResponseStatus.SQL_ERROR, requestResult.StatusCode);
        }

        [Fact]
        public void TestAcceptRequestFail()
        {
            int idFriendship = 1;

            var requestAcceptResult = FriendshipDao.AcceptRequest(idFriendship);

            Assert.Equal(ResponseStatus.SQL_ERROR, requestAcceptResult.StatusCode);
        }

        [Fact]
        public void TestDeclineRequestFail()
        {
            int idFriendship = 1;

            var requestDeclineResult = FriendshipDao.DeclineRequest(idFriendship);

            Assert.Equal(ResponseStatus.SQL_ERROR, requestDeclineResult.StatusCode);
        }

        [Fact]
        public void TestGetRequestsByIdFail()
        {
            int idUser = 1;

            var requests = FriendshipDao.GetRequestsById(idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, requests.StatusCode);
        }

        [Fact]
        public void TestGetFriendsByIdFail()
        {
            int idUser = 1;

            var friends = FriendshipDao.GetFriendsById(idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, friends.StatusCode);
        }

        [Fact]
        public void TestAddScorePointsFail()
        {
            string userNickname = "adjcg15";

            var scoreResult = MatchDao.AddScorePoints(userNickname);

            Assert.Equal(ResponseStatus.SQL_ERROR, scoreResult.StatusCode);
        }

        [Fact]
        public void TestGetTopPlayersFail()
        {
            string searchQuery = "";
            int numberOfPlayers = 10;
            
            var players = PlayerDao.GetTopPlayers(searchQuery, numberOfPlayers);

            Assert.Equal(ResponseStatus.SQL_ERROR, players.StatusCode);
        }

        [Fact]
        public void TestCheckPlayerPermanentBanFail()
        {
            string playerEmail = "adjcg15@gmail.com";
            
            var banVerification = PlayerDao.CheckPlayerPermanentBan(playerEmail);

            Assert.Equal(ResponseStatus.SQL_ERROR, banVerification.StatusCode);
        }

        [Fact]
        public void TestCheckPlayerTemporalBanFail()
        {
            string playerEmail = "adjcg15@gmail.com";

            var banLimitDate = PlayerDao.CheckPlayerTemporalBan(playerEmail);

            Assert.Equal(ResponseStatus.SQL_ERROR, banLimitDate.StatusCode);
        }

        [Fact]
        public void TestAddPlayerReportFail()
        {
            Report playerReport = new Report
            {
                idReportedUser = 1,
                comment = "Report comment example",
                idReportType = 1
            };

            var reportGeneratedResponse = ReportDao.AddPlayerReport(playerReport);

            Assert.Equal(ResponseStatus.SQL_ERROR, reportGeneratedResponse.StatusCode);
        }

        [Fact]
        public void TestGetReportsByUserIdFail()
        {
            int idUser = 1;

            var reports = ReportDao.GetReportsByUserId(idUser);

            Assert.Equal(ResponseStatus.SQL_ERROR, reports.StatusCode);
        }
    }

    public class DBOfflineTestsFixture
    {
        public DBOfflineTestsFixture()
        {
            ServerLogger.ConfigureLogger();
        }
    }
}
