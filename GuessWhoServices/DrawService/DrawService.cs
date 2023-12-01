using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IDrawService
    {
        private static readonly object lockObject = new object();
        private static Dictionary<string, DrawInformation> playersDrawInformation = new Dictionary<string, DrawInformation>();

        public void SendDraw(List<SerializedLine> localDrawMap, string matchCode)
        {
            lock (lockObject)
            {
                if (playersDrawInformation.ContainsKey(matchCode))
                {
                    var currentPlayersDraw = playersDrawInformation[matchCode];
                    var currentChannel = OperationContext.Current.GetCallbackChannel<IDrawServiceCallback>();

                    if (currentChannel.GetHashCode() == currentPlayersDraw.PlayerOneChannel.GetHashCode())
                    {
                        NotifyOtherPlayerDraw(currentPlayersDraw.PlayerTwoChannel, localDrawMap);
                    }
                    else if (currentChannel.GetHashCode() == currentPlayersDraw.PlayerTwoChannel.GetHashCode())
                    {
                        NotifyOtherPlayerDraw(currentPlayersDraw.PlayerOneChannel, localDrawMap);
                    }

                    Console.WriteLine(currentPlayersDraw.PlayerTwoDraw.GetHashCode() + " " + currentPlayersDraw.PlayerOneDraw.GetHashCode())
                }

                
            }
        }

        public void SubscribeToDrawService(string matchCode)
        {
            lock (lockObject)
            {
                var currentChannel = OperationContext.Current.GetCallbackChannel<IDrawServiceCallback>();
                Console.WriteLine(currentChannel.GetHashCode() + " canal agregado a DrawService");

                if (!playersDrawInformation.ContainsKey(matchCode))
                {
                    playersDrawInformation[matchCode] = new DrawInformation
                    {
                        PlayerOneChannel = currentChannel
                    };
                }
                else
                {
                    playersDrawInformation[matchCode].PlayerTwoChannel = currentChannel;
                }
            }
        }

        public void UnsubscribeFromDrawService(string matchCode)
        {
            lock (lockObject)
            {
                var currentChannel = OperationContext.Current.GetCallbackChannel<IDrawServiceCallback>();
                Console.WriteLine(currentChannel.GetHashCode() + " canal eliminado de DrawService");

                if (playersDrawInformation.ContainsKey(matchCode))
                {
                    var drawInfo = playersDrawInformation[matchCode];

                    if (drawInfo.PlayerOneChannel == currentChannel)
                    {
                        drawInfo.PlayerOneChannel = null;
                    }
                    else if (drawInfo.PlayerTwoChannel == currentChannel)
                    {
                        drawInfo.PlayerTwoChannel = null;
                    }

                    if (drawInfo.PlayerOneChannel == null && drawInfo.PlayerTwoChannel == null)
                    {
                        playersDrawInformation.Remove(matchCode);
                    }
                }
            }
        }

        private void NotifyOtherPlayerDraw(IDrawServiceCallback otherPlayerChannel, List<SerializedLine> localDrawMap)
        {
            lock (lockObject)
            {
                if (otherPlayerChannel != null)
                {
                    otherPlayerChannel.DrawReceived(localDrawMap);
                } 
            }
        }
    }

}
