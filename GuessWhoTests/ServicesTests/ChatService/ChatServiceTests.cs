using GuessWhoDataAccess;
using GuessWhoTests.GameServices;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Xunit;

namespace GuessWhoTests.ServicesTests.ChatService
{
    public class ChatServiceTests : IDisposable
    {
        private static ChatServiceClient hostProxy;
        private static ChatServiceClient guestProxy;
        private static MockChatServiceClient hostClient;
        private static MockChatServiceClient guestClient;
        private static string chatRoomCode = "8jias12k5l";
        private static bool isGuestInChat = true;

        public ChatServiceTests()
        {
            hostClient = new MockChatServiceClient();
            hostProxy = new ChatServiceClient(new InstanceContext(hostClient));

            guestClient = new MockChatServiceClient();
            guestProxy = new ChatServiceClient(new InstanceContext(guestClient));
        }

        public void Dispose()
        {
            hostProxy?.LeaveChatRoom(chatRoomCode);
            
            if(isGuestInChat)
            {
                guestProxy?.LeaveChatRoom(chatRoomCode);
            }
        }

        [Fact]
        public async void TestSendMessageFromHostSuccess()
        {
            hostProxy.EnterToChatRoom(chatRoomCode);
            guestProxy.EnterToChatRoom(chatRoomCode);

            string message = "Hola!";

            var messageSent = hostProxy.SendMessage(chatRoomCode, message);

            await Task.Delay(1500);
            Assert.True(messageSent.Value);
            Assert.Equal(message, guestClient.LatestMessage);
        }

        [Fact]
        public async void TestSendMessageFromGuestSuccess()
        {
            hostProxy.EnterToChatRoom(chatRoomCode);
            guestProxy.EnterToChatRoom(chatRoomCode);

            string message = "¡Cómo estás?";

            var messageSent = guestProxy.SendMessage(chatRoomCode, message);

            await Task.Delay(1500);
            Assert.True(messageSent.Value);
            Assert.Equal(message, hostClient.LatestMessage);
        }

        [Fact]
        public async void TestSendMessageFromHostMultipleTimesSuccess()
        {
            hostProxy.EnterToChatRoom(chatRoomCode);
            guestProxy.EnterToChatRoom(chatRoomCode);

            string lastMessage = "Adiós";

            hostProxy.SendMessage(chatRoomCode, "Hola");
            hostProxy.SendMessage(chatRoomCode, "Buenas tardes");
            hostProxy.SendMessage(chatRoomCode, "Cómo va tu día");
            var lastMessageSent = hostProxy.SendMessage(chatRoomCode, lastMessage);

            await Task.Delay(1500);
            Assert.True(lastMessageSent.Value);
            Assert.Equal(lastMessage, guestClient.LatestMessage);
            Assert.Equal(4, guestClient.TotalMessageReceived);
        }

        [Fact]
        public async void TestSendMessageFromGuestMultipleTimesSuccess()
        {
            hostProxy.EnterToChatRoom(chatRoomCode);
            guestProxy.EnterToChatRoom(chatRoomCode);

            string lastMessage = "Adiós";

            guestProxy.SendMessage(chatRoomCode, "Hola");
            guestProxy.SendMessage(chatRoomCode, "Buenas tardes");
            guestProxy.SendMessage(chatRoomCode, "Cómo va tu día");
            var lastMessageSent = guestProxy.SendMessage(chatRoomCode, lastMessage);

            await Task.Delay(1500);
            Assert.True(lastMessageSent.Value);
            Assert.Equal(lastMessage, hostClient.LatestMessage);
            Assert.Equal(4, hostClient.TotalMessageReceived);
        }

        [Fact]
        public async void TestSendMessageSimultaneouslySuccess()
        {
            hostProxy.EnterToChatRoom(chatRoomCode);
            guestProxy.EnterToChatRoom(chatRoomCode);

            string message = "¡Hola!";

            var hostMessageSent = hostProxy.SendMessage(chatRoomCode, message);
            var guestMessageSent = guestProxy.SendMessage(chatRoomCode, message);

            await Task.Delay(1500);
            Assert.True(hostMessageSent.Value);
            Assert.True(guestMessageSent.Value);
            Assert.Equal(message, hostClient.LatestMessage);
            Assert.Equal(message, guestClient.LatestMessage);
        }

        [Fact]
        public async void TestSendMessageChatRoomNonExistentFail()
        {
            hostProxy.EnterToChatRoom(chatRoomCode);
            guestProxy.EnterToChatRoom(chatRoomCode);

            string message = "¡Hola!";
            string nonExistentChatRoom = "8iklklo90q";

            var messageSent = hostProxy.SendMessage(nonExistentChatRoom, message);

            await Task.Delay(1500);
            Assert.False(messageSent.Value);
            Assert.Equal(ResponseStatus.VALIDATION_ERROR, messageSent.StatusCode);
        }

        [Fact]
        public async void TestSendMessageEmptyChatRoomFail()
        {
            hostProxy.EnterToChatRoom(chatRoomCode);
            isGuestInChat = false;

            string message = "¡Hola!";

            var messageSent = hostProxy.SendMessage(chatRoomCode, message);

            await Task.Delay(1500);
            Assert.False(messageSent.Value);
            Assert.Equal(ResponseStatus.OK, messageSent.StatusCode);
        }
    }
}
