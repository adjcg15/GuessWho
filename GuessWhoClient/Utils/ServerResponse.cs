using GuessWhoClient.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoClient.Utils
{
    public class ServerResponse
    {
        public static string GetTitleFromStatusCode(GameServices.ResponseStatus statusCode)
        {
            Dictionary<GameServices.ResponseStatus, string> titleOptions = new Dictionary<GameServices.ResponseStatus, string>();

            titleOptions.Add(GameServices.ResponseStatus.UPDATE_ERROR, Properties.Resources.txtUpdateErrorTitle);
            titleOptions.Add(GameServices.ResponseStatus.VALIDATION_ERROR, Properties.Resources.txtValidationErrorTitle);
            titleOptions.Add(GameServices.ResponseStatus.SQL_ERROR, Properties.Resources.txtSQLErrorTitle);

            string title = "";
            if(titleOptions.ContainsKey(statusCode))
            {
                title = titleOptions[statusCode];
            }

            return title;
        }

        public static string GetMessageFromStatusCode(GameServices.ResponseStatus statusCode)
        {
            Dictionary<GameServices.ResponseStatus, string> messageOptions = new Dictionary<GameServices.ResponseStatus, string>();

            messageOptions.Add(GameServices.ResponseStatus.UPDATE_ERROR, Properties.Resources.txtUpdateErrorMessage);
            messageOptions.Add(GameServices.ResponseStatus.VALIDATION_ERROR, Properties.Resources.txtValidationErrorMessage);
            messageOptions.Add(GameServices.ResponseStatus.SQL_ERROR, Properties.Resources.txtSQLErrorMessage);

            string message = "";
            if (messageOptions.ContainsKey(statusCode))
            {
                message = messageOptions[statusCode];
            }

            return message;
        }
    }
}
