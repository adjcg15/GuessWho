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
            if (!subscribers.Contains(callback))
            {
                subscribers.Add(callback);
            }
        }

        public void Unsubscribe()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IUsersCallback>();
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
