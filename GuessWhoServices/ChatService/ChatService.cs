using GuessWhoDataAccess;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IChatService
    {
        private readonly static Dictionary<string, ChatRoom> chatRooms = new Dictionary<string, ChatRoom>();

        public void RegisterChatRoom(string chatRoomCode)
        {
            var newChatRoom = new ChatRoom();
            newChatRoom.HostChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            if(chatRooms.ContainsKey(chatRoomCode))
            {
                chatRooms.Remove(chatRoomCode);
            }

            chatRooms.Add(chatRoomCode, newChatRoom);
        }

        public void DeleteChatRoom(string chatRoomCode)
        {
            if (chatRooms.ContainsKey(chatRoomCode))
            {
                chatRooms.Remove(chatRoomCode);
            }
        }

        public void JoinChatRoom(string chatRoomCode)
        {
            if (chatRooms.ContainsKey(chatRoomCode))
            {
                IChatCallback guestChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();
                chatRooms[chatRoomCode].GuestChannel = guestChannel;
            }
        }

        public Response<bool> SendMessage(string chatRoomCode, string message)
        {
            Response<bool> response = new Response<bool>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = false
            };

            if (chatRooms.ContainsKey(chatRoomCode))
            {
                var storedChatRoom = chatRooms[chatRoomCode];
                var senderChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();

                response.StatusCode = ResponseStatus.OK;
                response.Value = true;
                try
                {
                    bool isHostSendingMessage = senderChannel.GetHashCode() == storedChatRoom.HostChannel.GetHashCode();
                    if (isHostSendingMessage)
                    {
                        if (storedChatRoom.GuestChannel != null)
                        {
                            storedChatRoom.GuestChannel.NewMessageReceived(message);
                        }
                        else
                        {
                            response.Value = false;
                        }
                    }
                    else
                    {
                        storedChatRoom.HostChannel.NewMessageReceived(message);
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;
                    response.Value = false;

                    chatRooms.Remove(chatRoomCode);
                }
            }

            return response;
        }
    }
}
