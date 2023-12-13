using GuessWhoDataAccess;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IChatService
    {
        private readonly static Dictionary<string, ChatRoom> chatRooms = new Dictionary<string, ChatRoom>();

        public void EnterToChatRoom(string chatRoomCode)
        {
            bool isChatRoomAlreadyRegistered = chatRooms.ContainsKey(chatRoomCode);

            if (isChatRoomAlreadyRegistered)
            {
                JoinChatRoom(chatRoomCode);
            }
            else
            {
                RegisterChatRoom(chatRoomCode);
            }
        }

        private void JoinChatRoom(string chatRoomCode)
        {
            IChatCallback userChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            ChatRoom chatRoom = chatRooms[chatRoomCode];

            chatRoom.SecondUserInChatRoomChannel = userChannel;
        }

        private void RegisterChatRoom(string chatRoomCode)
        {
            IChatCallback userChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            ChatRoom newChatRoom = new ChatRoom();
            newChatRoom.FirstUserInChatRoomChannel = userChannel;

            chatRooms.Add(chatRoomCode, newChatRoom);
        }

        public void LeaveChatRoom(string chatRoomCode)
        {
            ChatRoom chatRoom = chatRooms[chatRoomCode];

            if(chatRoom != null)
            {
                bool isLastUserLeavingChatRoom = chatRoom.FirstUserInChatRoomChannel == null || chatRoom.SecondUserInChatRoomChannel == null;
                
                if (isLastUserLeavingChatRoom)
                {
                    DeleteChatRoom(chatRoomCode);
                }
                else
                {
                    RemoveUserFromChatRoom(chatRoomCode);
                }
            }
        }

        public void DeleteChatRoom(string chatRoomCode)
        {
            chatRooms.Remove(chatRoomCode);
        }

        public void RemoveUserFromChatRoom(string chatRoomCode)
        {
            ChatRoom chatRoom = chatRooms[chatRoomCode];
            IChatCallback userChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();

            if(userChannel.GetHashCode() == chatRoom.FirstUserInChatRoomChannel.GetHashCode())
            {
                chatRoom.FirstUserInChatRoomChannel = null;
            }
            else
            {
                chatRoom.SecondUserInChatRoomChannel = null;
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
                ChatRoom storedChatRoom = chatRooms[chatRoomCode];
                IChatCallback userChannel = OperationContext.Current.GetCallbackChannel<IChatCallback>();

                response.StatusCode = ResponseStatus.OK;
                response.Value = true;
                try
                {
                    bool isFirstPlayerToJoinSendingMessage = userChannel.GetHashCode() == storedChatRoom.FirstUserInChatRoomChannel.GetHashCode();
                    if (isFirstPlayerToJoinSendingMessage)
                    {
                        if (storedChatRoom.SecondUserInChatRoomChannel != null)
                        {
                            storedChatRoom.SecondUserInChatRoomChannel.NewMessageReceived(message);
                        }
                        else
                        {
                            response.Value = false;
                        }
                    }
                    else
                    {
                        storedChatRoom.FirstUserInChatRoomChannel.NewMessageReceived(message);
                    }
                }
                catch (CommunicationObjectAbortedException ex)
                {
                    ServerLogger.Instance.Error(ex.Message);

                    response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;
                    response.Value = false;

                    chatRooms.Remove(chatRoomCode);
                }
            }

            return response;
        }
    }
}
