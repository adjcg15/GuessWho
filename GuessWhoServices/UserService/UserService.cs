using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IUserService
    {
        private readonly static List<IUsersCallback> clientChannels = new List<IUsersCallback>();
        private readonly static List<string> activeUsers = new List<string>();

        public void Subscribe()
        {
            var channel = OperationContext.Current.GetCallbackChannel<IUsersCallback>();
            Console.WriteLine("Suscribiendo a lista usuarios activos " + channel.GetHashCode());
            if (!clientChannels.Contains(channel))
            {
                clientChannels.Add(channel);
            }
        }

        public void Unsubscribe()
        {
            var channel = OperationContext.Current.GetCallbackChannel<IUsersCallback>();
            Console.WriteLine("Quitando suscripción a lista usuarios activos " + channel.GetHashCode());
            if (clientChannels.Contains(channel))
            {
                clientChannels.Remove(channel);
            }
        }

        public List<string> GetActiveUsers()
        {
            return activeUsers;
        }

        public static void UpdateUserStatus(string userNickname, bool isOnline)
        {
            string userNicknameStored = activeUsers.Find(n => n == userNickname);

            if (isOnline)
            {
                if (string.IsNullOrEmpty(userNicknameStored))
                {
                    activeUsers.Add(userNickname);
                }
            }
            else
            {
                activeUsers.Remove(userNicknameStored);
            }

            foreach (var subscriber in clientChannels)
            {
                Console.WriteLine(
                    "Avisando a suscriptor de lista de usuarios activos " + subscriber.GetHashCode() + 
                    " que usuario " + userNickname + 
                    (isOnline ? " inicia sesión" : " cierra sesión")
                );
                try
                {
                    subscriber.UserStatusChanged(userNickname, isOnline);
                }
                catch (CommunicationObjectAbortedException)
                {
                    clientChannels.Remove(subscriber);
                }
            }
        } 
    }
}
