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
            var callback = OperationContext.Current.GetCallbackChannel<IUsersCallback>();
            Console.WriteLine("Subscribe: " + callback.GetHashCode());
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }

        public void Unsubscribe()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IUsersCallback>();
            Console.WriteLine("Unsubscribe: " + callback.GetHashCode());
            if (subscribers.Contains(callback))
            {
                subscribers.Remove(callback);
            }
        }

        public List<ActiveUser> GetActiveUsers()
        {
            return activeUsers;
        }

        public static void UpdateUserStatus(ActiveUser user, bool isActive)
        {
            Console.WriteLine("Cambiando estado de " + user.Nickname + " a " + (isActive ? "activo" : "inactivo"));
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

            Console.WriteLine("Total de jugadores suscriptores: " + subscribers.Count);
            Console.WriteLine("Total de jugadores activos: " + activeUsers.Count);
            foreach (var subscriber in subscribers)
            {
                Console.WriteLine("Enviando notificación al suscriptor " + subscriber.GetHashCode());
                subscriber.UserStatusChanged(user, isActive);
            }
        }
    }
}
