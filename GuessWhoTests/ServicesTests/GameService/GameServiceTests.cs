using GuessWhoDataAccess;
using GuessWhoTests.GameServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GuessWhoTests.ServicesTests.GameService
{
    public class GameServiceTests : IDisposable
    {
        private static GameServiceClient clientHostJoinGame;
        private static GameServiceClient clientGuestJoinGame;
        private static MockGameServiceClient mockCallbackHostJoinGameSuccess;
        private static MockGameServiceClient mockCallbackGuestJoinGameSuccess;

        private static GameServiceClient clientJoinGame;
        private static MockGameServiceClient mockCallbackJoinGameFail;

        private static GameServiceClient clientHostJoinGameTournamentFail;
        private static GameServiceClient clientGuestJoinGameTournamentFail;
        private static MockGameServiceClient mockCallbackHostJoinGameTournamentFail;
        private static MockGameServiceClient mockCallbackGuestJoinGameTournamentFail;

        private static GameServiceClient clientHostExitGameSuccess;
        private static GameServiceClient clientGuestExitGameSuccess;
        private static MockGameServiceClient mockCallbackHostExitGameSuccess;
        private static MockGameServiceClient mockCallbackGuestExitGameSuccess;

        public string InvitationCodeJoinSuccess { get; set; }
        public string InvitationCodeExitSuccess { get; set; }
        public string InvitationCodeTournamentFail {  get; set; }  


        public GameServiceTests()
        {
            mockCallbackHostJoinGameSuccess = new MockGameServiceClient();
            clientHostJoinGame = new GameServiceClient(new InstanceContext(mockCallbackHostJoinGameSuccess));
            var invitationCodeJoin = clientHostJoinGame.CreateMatch("", false);

            InvitationCodeJoinSuccess = invitationCodeJoin.Value;

            mockCallbackGuestJoinGameSuccess = new MockGameServiceClient();
            clientGuestJoinGame = new GameServiceClient(new InstanceContext(mockCallbackGuestJoinGameSuccess));

            mockCallbackJoinGameFail = new MockGameServiceClient();
            clientJoinGame = new GameServiceClient(new InstanceContext(mockCallbackJoinGameFail));

            mockCallbackHostJoinGameTournamentFail = new MockGameServiceClient();
            clientHostJoinGameTournamentFail = new GameServiceClient(new InstanceContext(mockCallbackHostJoinGameTournamentFail));
            var invitationCodeTournament = clientHostJoinGameTournamentFail.CreateMatch("", true);

            InvitationCodeTournamentFail = invitationCodeTournament.Value;

            mockCallbackGuestJoinGameTournamentFail = new MockGameServiceClient();
            clientGuestJoinGameTournamentFail = new GameServiceClient(new InstanceContext(mockCallbackGuestJoinGameTournamentFail));

            mockCallbackHostExitGameSuccess = new MockGameServiceClient();
            clientHostExitGameSuccess = new GameServiceClient(new InstanceContext(mockCallbackHostExitGameSuccess));
            var invitationCodeExit = clientHostExitGameSuccess.CreateMatch("", false);

            InvitationCodeExitSuccess = invitationCodeExit.Value;

            mockCallbackGuestExitGameSuccess = new MockGameServiceClient();
            clientGuestExitGameSuccess = new GameServiceClient(new InstanceContext(mockCallbackGuestExitGameSuccess));
            clientGuestExitGameSuccess.JoinGame(InvitationCodeExitSuccess, "");
        }

        [Fact]
        public async void TestJoinGameSuccess()
        {
            clientGuestJoinGame.JoinGame(InvitationCodeJoinSuccess, "");

            await Task.Delay(2000);
            Assert.True(mockCallbackHostJoinGameSuccess.HasJoinGame);
        }

        [Fact]
        public async void TestJoinGameInvalidCodeFail()
        {
            string INVALID_INVITATION_CODE = "--------";

            var isInMatch = clientJoinGame.JoinGame(INVALID_INVITATION_CODE, "");

            await Task.Delay(2000);
            Assert.Equal(ResponseStatus.VALIDATION_ERROR, isInMatch.StatusCode);
            Assert.Null(isInMatch.Value);
        }

        [Fact]
        public async void TestJoinGameTournamentFail()
        {
            var response = clientGuestJoinGameTournamentFail.JoinGame(InvitationCodeTournamentFail, "");

            await Task.Delay(2000);
            Assert.False(mockCallbackHostJoinGameTournamentFail.HasJoinGame);
            Assert.Equal(ResponseStatus.NOT_ALLOWED, response.StatusCode);
            Assert.Null(response.Value);
        }

        [Fact]
        public async void TestExitGameSuccess()
        {
            clientGuestExitGameSuccess.ExitGame(InvitationCodeExitSuccess);

            await Task.Delay(2000);
            Assert.True(mockCallbackHostExitGameSuccess.HasLeftGame);
        }

        public void Dispose()
        {
            clientGuestJoinGame.ExitGame(InvitationCodeJoinSuccess);
            clientHostJoinGame.FinishGame(InvitationCodeJoinSuccess);

            clientGuestExitGameSuccess.ExitGame(InvitationCodeExitSuccess);
            clientHostExitGameSuccess.FinishGame(InvitationCodeExitSuccess);

            clientHostJoinGameTournamentFail.FinishGame(InvitationCodeTournamentFail);
        }
    }
}
