using GuessWhoTests.GameServices;
using System.ServiceModel;
using System.Threading.Tasks;
using Xunit;

namespace GuessWhoTests.ServicesTests.MatchStatusService
{
    public class MatchStatusServiceTests
    {
        private static MatchStatusServiceClient hostProxy;
        private static MatchStatusServiceClient guestProxy;
        private static MockMatchStatusClient hostClient;
        private static MockMatchStatusClient guestClient;
        private static string matchCode = "8jias12k5l";

        public MatchStatusServiceTests()
        {
            hostClient = new MockMatchStatusClient();
            hostProxy = new MatchStatusServiceClient(new InstanceContext(hostClient));
            hostProxy.ListenMatchStatus(matchCode, "thehost");

            guestClient = new MockMatchStatusClient();
            guestProxy = new MatchStatusServiceClient(new InstanceContext(guestClient));
            guestProxy.ListenMatchStatus(matchCode, "theguest");
        }

        [Fact]
        public async void TestStartCharacterSelectionSuccess()
        {
            hostProxy.StartCharacterSelection(matchCode);

            await Task.Delay(1000);
            Assert.True(guestClient.RedirectedToCharacterSelection);
        }
    }
}
