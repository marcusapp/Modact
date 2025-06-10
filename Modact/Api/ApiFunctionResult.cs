namespace Modact
{
    [Serializable]
    public class ApiFunctionResult
    {
        /// <summary>
        /// Is success of function run result
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Data of run result
        /// </summary>
        public object? Data { get; set; }
        /// <summary>
        /// Datas of run result
        /// </summary>
        ///public ConcurrentDictionary<string, object> Datas { get; set; }

        public ApiFunctionResult()
        {
            Success = false;
        }
    }
}