using GuessWhoDataAccess;
using GuessWhoServices.Utils;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IMatchService
    {
        private static Dictionary<string, MatchInformation> matches = new Dictionary<string, MatchInformation>();

        public Response<string> CreateMatch(string hostNickname)
        {
            var response = new Response<string>
            {
                StatusCode = ResponseStatus.OK,
                Value = ""
            };

            string invitationCode;
            do
            {
                invitationCode = Game.GenerateInvitationCode();
            }
            while (matches.ContainsKey(invitationCode));

            var match = new MatchInformation();
            match.HostNickname = hostNickname;
            match.HostChannel = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
            matches[invitationCode] = match;

            response.Value = invitationCode;

            Console.WriteLine("Se agregó al map el host: " + matches[invitationCode].HostNickname);
            Console.WriteLine("Dicho host tiene el canal: " + matches[invitationCode].HostChannel.GetHashCode());
            Console.WriteLine("Y el código de invitación es: " + invitationCode);
            return response;
        }
    }
}
