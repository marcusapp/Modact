namespace Modact
{
    public class ApiAction
    {
        public bool IsAsync { get; set; } = false;
        public bool IsTrans { get; set; } = true;
        public List<ApiFunction> Functions { get; set; }
    }
}
