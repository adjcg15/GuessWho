using System.Runtime.Serialization;

namespace GuessWhoDataAccess
{
    [DataContract(Name = "{0}Response")]
    public class Response<T>
    {
        private ResponseStatus statusCode;
        private T value;

        [DataMember]
        public ResponseStatus StatusCode { get; set; }

        [DataMember]
        public T Value { get; set; }
    }

    public enum ResponseStatus
    {
        OK = 200,
        VALIDATION_ERROR = 400,
        UPDATE_ERROR = 500,
        SQL_ERROR = 501,
    }
}
