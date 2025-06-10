namespace Modact
{
    [Serializable]
    public class ApiResponse
    {
        /// <summary>
        /// Response ID
        /// </summary>
        public string ResponseId { get; set; }
        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestId { get; set; }
        /// <summary>
        /// Response time
        /// </summary>
        public DateTime ResponseTime { get; set; }
        public string AppId { get; set; }
        public string AppVersion { get; set; }
        public int Success { get; set; }
        public int Fatal { get; set; }
        public int Error { get; set; }
        public int Warn { get; set; }
        public List<ApiMessage> Messages { get; set; }
        public Dictionary<string, ApiFunctionResult> Results { get; set; }
        public Dictionary<string, object?> Session { get; set; }

        public ApiResponse()
        {
            this.ResponseTime = DateTime.Now;
            this.Success = 0;
            this.Fatal = 0;
            this.Error = 0;
            this.Warn = 0;
            this.Messages = new();
            this.Results = new();
        }

        public ApiResponse(string responseId, string requestId)
        {
            this.ResponseId = responseId;
            this.RequestId = requestId;
            this.ResponseTime = DateTime.Now;
            this.Success = 0;
            this.Fatal = 0;
            this.Error = 0;
            this.Warn = 0;
            this.Messages = new();
            this.Results = new();
        }
        /// <summary>
        /// Add ApiFunctionMessage.
        /// </summary>
        /// <param name="apiMessage">ApiFunction message</param>
        public void AddMessage(ApiMessage apiMessage)
        {
            this.Messages.Add(apiMessage);
            switch (apiMessage.Type)
            {
                case ApiMessageType.Fatal: this.Fatal++;
                break;
                case ApiMessageType.Error: this.Error++;
                break;
                case ApiMessageType.Warn: this.Warn++;
                break;                       
            }
        }
        /// <summary>
        /// Add ApiFunctionMessages.
        /// </summary>
        /// <param name="apiMessages">ApiFunction messages</param>
        public void AddMessages(IEnumerable<ApiMessage> apiMessages)
        {
            this.Messages.AddRange(apiMessages);
            foreach (ApiMessage apiMessage in apiMessages) 
            {
                switch (apiMessage.Type)
                {
                    case ApiMessageType.Fatal:
                        this.Fatal++;
                        break;
                    case ApiMessageType.Error:
                        this.Error++;
                        break;
                    case ApiMessageType.Warn:
                        this.Warn++;
                        break;
                }
            }
        }
        /// <summary>
        /// Add ApiFunctionResult.
        /// </summary>
        /// <param name="id">ApiFunction Id</param>
        /// <param name="apiResult">ApiFunction result</param>
        public void AddResult(string id, ApiFunctionResult apiResult)
        {
            this.Results.TryAdd(id, apiResult);
            if (apiResult.Success)
            {
                this.Success++;
            }
        }
    }
}