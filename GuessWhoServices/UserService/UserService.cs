using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IUserService
    {
        private static List<IUsersCallback> subscribers = new List<IUsersCallback>();
        private static List<ActiveUser> activeUsers = new List<ActiveUser>();

        public void Subscribe()
        {
            var channel = OperationContext.Current.GetCallbackChannel<IUsersCallback>();
            Console.WriteLine("Suscribiendo a lista usuarios activos " + channel.GetHashCode());
            if (!subscribers.Contains(channel))
            {
                subscribers.Add(channel);
            }
        }

        public void Unsubscribe()
        {
            var channel = OperationContext.Current.GetCallbackChannel<IUsersCallback>();
            Console.WriteLine("Quitando suscripción a lista usuarios activos " + channel.GetHashCode());
            if (subscribers.Contains(channel))
            {
                subscribers.Remove(channel);
            }
        }

        public List<ActiveUser> GetActiveUsers()
        {
            return activeUsers;
        }

        public static void UpdateUserStatus(ActiveUser user, bool isActive)
        {
            if (isActive)
            {
                if (activeUsers.Find(u => u.Nickname == user.Nickname) == null)
                {
                    activeUsers.Add(user);
                }
            }
            else
            {
                activeUsers.Remove(activeUsers.Find(u => u.Nickname == user.Nickname));
            }

            foreach (var subscriber in subscribers)
            {
                Console.WriteLine(
                    "Avisando a suscriptor de lista de usuarios activos " + subscriber.GetHashCode() + 
                    " que usuario " + user.Nickname + 
                    (isActive ? " inicia sesión" : " cierra sesión")
                );
                try
                {
                    subscriber.UserStatusChanged(user, isActive);
                }
                catch (CommunicationObjectAbortedException)
                {
                    subscribers.Remove(subscriber);
                }
            }
        }
    }
}
