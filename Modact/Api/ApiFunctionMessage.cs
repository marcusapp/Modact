namespace Modact
{

    [Serializable]
    public class ApiFunctionMessage
    {
        /// <summary>
        /// Messages of API Function
        /// </summary>
        public List<ApiMessage> Messages { get; set; } = new List<ApiMessage>();
    }
}
