using GuessWhoClient.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GuessWhoClient.Utils
{
    public class ServerResponse
    {
        public static string GetTitleFromStatusCode(GameServices.ResponseStatus statusCode)
        {
            Dictionary<GameServices.ResponseStatus, string> titleOptions = new Dictionary<GameServices.ResponseStatus, string>();

            titleOptions.Add(GameServices.ResponseStatus.UPDATE_ERROR, Resources.txtUpdateErrorTitle);
            titleOptions.Add(GameServices.ResponseStatus.VALIDATION_ERROR, Resources.txtValidationErrorTitle);
            titleOptions.Add(GameServices.ResponseStatus.SQL_ERROR, Resources.txtSQLErrorTitle);

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

            messageOptions.Add(GameServices.ResponseStatus.UPDATE_ERROR, Resources.txtUpdateErrorMessage);
            messageOptions.Add(GameServices.ResponseStatus.VALIDATION_ERROR, Resources.txtValidationErrorMessage);
            messageOptions.Add(GameServices.ResponseStatus.SQL_ERROR, Resources.txtSQLErrorMessage);

            string message = "";
            if (messageOptions.ContainsKey(statusCode))
            {
                message = messageOptions[statusCode];
            }

            return message;
        }

        public static void ShowServerDownMessage()
        {
            MessageBox.Show(
                Resources.msgbConnectionServerErrorMessage,
                Resources.msgbConnectionServerErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        public static void ShowConnectionLostMessage()
        {
            MessageBox.Show(
                Resources.msgbNetworkConnectionErrorMessage,
                Resources.msgbNetworkConnectionErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}
