﻿using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IMatchCallback))]
    public interface IMatchService
    {
        [OperationContract]
        Response<string> CreateMatch(string hostNickname);
    }

    [ServiceContract]
    public interface IMatchCallback
    {
        
    }
}
