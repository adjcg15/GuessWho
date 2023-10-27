using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace GuessWhoDataAccess
{
    public class PlayerDAO
    {
        public static Response<List<TopPlayer>> GetTopPlayers(int numberOfPlayers)
        {
            Response<List<TopPlayer>> response = new Response<List<TopPlayer>>();
            response.Value = new List<TopPlayer>();
            response.StatusCode = ResponseStatus.OK;

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var topPlayers = context.Matches
                        .Where(m => m.status == "finished")
                        .GroupBy(m => m.idWinner)
                        .Select(g => new
                        {
                            UserId = g.Key,
                            TotalScore = g.Sum(m => m.score)
                        })
                        .OrderByDescending(tp => tp.TotalScore)
                        .Take(numberOfPlayers)
                        .ToList();

                    foreach (var player in topPlayers)
                    {
                        var user = context.Users.FirstOrDefault(u => u.idUser == player.UserId);
                        if (user != null && player.TotalScore.HasValue)
                        {
                            response.Value.Add(new TopPlayer
                            {
                                Nickname = user.nickname,
                                Score = (int)player.TotalScore
                            });
                        }
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                response.StatusCode = ResponseStatus.UPDATE_ERROR;
            }
            catch (DbEntityValidationException ex)
            {
                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
            }
            catch (SqlException ex)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }
    }

    [DataContract]
    public class TopPlayer
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public int Score { get; set; }
    }
}
