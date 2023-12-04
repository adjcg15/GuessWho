using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IDrawService
    {
        private static Dictionary<string, DrawInformation> playersDrawInformation = new Dictionary<string, DrawInformation>();
        private static readonly object lockObject = new object();

        public void SendDraw(List<SerializedLine> draw, string matchCode)
        {
            lock (lockObject)
            {
                Console.WriteLine("Entrando a servicio de encío de dibujo");
                if (playersDrawInformation.ContainsKey(matchCode))
                {
                    var currentPlayersDraw = playersDrawInformation[matchCode];
                    var currentChannel = OperationContext.Current.GetCallbackChannel<IDrawServiceCallback>();

                    Console.WriteLine("Jugador " + currentChannel.GetHashCode() + " enviando dibujo en partida " + matchCode);
                    if (currentChannel.GetHashCode() == currentPlayersDraw.PlayerOneChannel.GetHashCode())
                    {
                        SendDrawToAdversary(currentPlayersDraw.PlayerTwoChannel, draw);
                    }
                    else if (currentChannel.GetHashCode() == currentPlayersDraw.PlayerTwoChannel.GetHashCode())
                    {
                        SendDrawToAdversary(currentPlayersDraw.PlayerOneChannel, draw);
                    }
                }
            }
        }

        public void SubscribeToDrawService(string matchCode)
        {
            lock (lockObject)
            {
                var currentChannel = OperationContext.Current.GetCallbackChannel<IDrawServiceCallback>();
                Console.WriteLine("Suscribiendo cliente " + currentChannel.GetHashCode() + " a DrawService en partida " + matchCode);

                if (!playersDrawInformation.ContainsKey(matchCode))
                {
                    Console.WriteLine("Registrando partida " + matchCode + " en DrawService");
                    playersDrawInformation[matchCode] = new DrawInformation
                    {
                        PlayerOneChannel = currentChannel
                    };
                }
                else
                {
                    Console.WriteLine("Uniendo subscriptor a partida " + matchCode + " en DrawService");
                    playersDrawInformation[matchCode].PlayerTwoChannel = currentChannel;
                }
            }
        }

        public void UnsubscribeFromDrawService(string matchCode)
        {
            lock (lockObject)
            {
                var currentChannel = OperationContext.Current.GetCallbackChannel<IDrawServiceCallback>();
                Console.WriteLine("Quitando suscripción de " + currentChannel.GetHashCode() + " de DrawService con código de partida " + matchCode);

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

        private void SendDrawToAdversary(IDrawServiceCallback adversaryChannel, List<SerializedLine> draw)
        {
            if (adversaryChannel != null)
            {
                Console.WriteLine("Enviando dibujo a " + adversaryChannel.GetHashCode());
                try
                {
                    adversaryChannel.DrawReceived(draw);
                }
                catch (CommunicationObjectAbortedException)
                {
                    //TO-DO: log exception
                }
            }
        }
    }

}
