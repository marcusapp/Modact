namespace Modact
{
    /// <summary>
    /// This exception not auto attach to API message list
    /// </summary>
    public class ApiNoMessageException : Exception
    {

    }

    /// <summary>
    /// This exception can attach the message code
    /// </summary>
    public class ApiMessageException : Exception
    {
        public string? Code { get; set; }

        public ApiMessageException(string? message, string? code) : base(message)
        {
            this.Code = code;
        }
    }
}
