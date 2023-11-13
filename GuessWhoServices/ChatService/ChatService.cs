using GuessWhoDataAccess;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IChatService
    {
        public Response<bool> SendMessage(string invitationCode, string message)
        {
            var response = new Response<bool>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = false
            };



            return response;
        }
    }
}
