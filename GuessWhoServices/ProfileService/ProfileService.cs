using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IProfileService
    {
        public Response<bool> UpdateUserProfileImage(byte[] newImage, int idUser) 
        {
            return UserDAO.UpdateUserProfileImage(newImage, idUser);
        }

        public Response<bool> UpdateUserNickname(string newNickname, int idUser)
        {
            Response<bool> response = new Response<bool>()
            {
                StatusCode = ResponseStatus.OK,
                Value = true,
            };

            DateTime currentDate = DateTime.Now;

            var lastTimeNicknameChangedResponse = UserDAO.GetLastTimeNicknameChangeById(idUser);

            if(lastTimeNicknameChangedResponse.StatusCode != ResponseStatus.OK)
            {
                response.StatusCode = lastTimeNicknameChangedResponse.StatusCode;
                response.Value = false;
            }
            else if(lastTimeNicknameChangedResponse.StatusCode == ResponseStatus.OK && 
                lastTimeNicknameChangedResponse.Value == DateTime.MaxValue)
            {
                response.Value = false;
            }
            else
            {
                if ((currentDate - lastTimeNicknameChangedResponse.Value).TotalDays >= 90)
                {
                    response = UserDAO.UpdateUserNickname(newNickname, idUser);
                }
                else
                {
                    response.Value = false;
                }
            }

            return response;
        }

        public Response<bool> UpdateUserFullName(string newFullName, int idUser)
        {
            return UserDAO.UpdateUserFullname(newFullName, idUser);
        }

        public Response<bool> UpdateUserPassword(string newPassword, int idAccount)
        {
            return UserDAO.UpdateAccountPassword(newPassword, idAccount);
        }
    }
}
