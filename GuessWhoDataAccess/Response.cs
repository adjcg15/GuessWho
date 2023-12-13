using System.Runtime.Serialization;

namespace GuessWhoDataAccess
{
    [DataContract(Name = "{0}Response")]
    public class Response<T>
    {
        [DataMember]
        public ResponseStatus StatusCode { get; set; }

        [DataMember]
        public T Value { get; set; }
    }

    public enum ResponseStatus
    {
        OK = 200,
        VALIDATION_ERROR = 400,
        CLIENT_CHANNEL_CONNECTION_ERROR = 401,
        NOT_ALLOWED = 402,
        UPDATE_ERROR = 500,
        SQL_ERROR = 501
    }
}
