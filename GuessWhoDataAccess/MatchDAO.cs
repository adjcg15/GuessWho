using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoDataAccess
{
    public class MatchDAO
    {
        public static Response<bool> AddScorePoints(string winnerNickname)
        {
            Console.WriteLine("Entrando a AddScorePoints Ganador: " + winnerNickname);

            var response = new Response<bool>
            {
                StatusCode = ResponseStatus.OK,
                Value = false
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var winner = context.Users.FirstOrDefault(a => a.nickname == winnerNickname);
                    
                    if(winner != null)
                    {
                        var match = new Match
                        {
                            idWinner = winner.idUser,
                            score = 5
                        };
                        context.Matches.Add(match);
                        context.SaveChanges();

                        response.Value = true;
                        Console.WriteLine("DAO: Sumando 5 puntos a " + winnerNickname);
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine("DbUpdateException");

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
                response.Value = false;
            }
            catch (DbEntityValidationException ex)
            {
                Console.WriteLine("DbEntityValidationException");

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                response.Value = false;
            }
            catch (SqlException ex)
            {
                Console.WriteLine("SqlException");

                response.StatusCode = ResponseStatus.SQL_ERROR;
                response.Value = false;
            }

            return response;
        }
    }
}
