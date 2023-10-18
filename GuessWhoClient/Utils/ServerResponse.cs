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
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
            Dictionary<GameServices.ResponseStatus, string> titleOptions = new Dictionary<GameServices.ResponseStatus, string>();

            titleOptions.Add(GameServices.ResponseStatus.UPDATE_ERROR, resourceManager.GetString("txtUpdateErrorTitle"));
            titleOptions.Add(GameServices.ResponseStatus.VALIDATION_ERROR, resourceManager.GetString("txtValidationErrorTitle"));
            titleOptions.Add(GameServices.ResponseStatus.SQL_ERROR, resourceManager.GetString("txtSQLErrorTitle"));

            string title = "";
            if(titleOptions.ContainsKey(statusCode))
            {
                title = titleOptions[statusCode];
            }

            return title;
        }

        public static string GetMessageFromStatusCode(GameServices.ResponseStatus statusCode)
        {
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
            Dictionary<GameServices.ResponseStatus, string> messageOptions = new Dictionary<GameServices.ResponseStatus, string>();

            messageOptions.Add(GameServices.ResponseStatus.UPDATE_ERROR, resourceManager.GetString("txtUpdateErrorMessage"));
            messageOptions.Add(GameServices.ResponseStatus.VALIDATION_ERROR, resourceManager.GetString("txtValidationErrorMessage"));
            messageOptions.Add(GameServices.ResponseStatus.SQL_ERROR, resourceManager.GetString("txtSQLErrorMessage"));

            string message = "";
            if (messageOptions.ContainsKey(statusCode))
            {
                message = messageOptions[statusCode];
            }

            return message;
        }
    }
}
