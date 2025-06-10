namespace Modact
{
    public interface IDto
    {

    }

    public interface ICreatableDto : IDto
    {
        DateTime? create_time { get; set; }
        DateTime? create_time_utc { get; set; }
        string? create_user_id { get; set; }
        string? create_user_name { get; set; }
        string? create_log_id { get; set; }
    }

    public interface IVoidableDto : ICreatableDto
    {
        bool? is_void { get; set; }
        DateTime? void_switch_time { get; set; }
        DateTime? void_switch_time_utc { get; set; }
        string? void_switch_user_id { get; set; }
        string? void_switch_user_name { get; set; }
        string? void_switch_log_id { get; set; }
        string? void_remark { get; set; }
    }

    public interface IUpdatableDto : IVoidableDto
    {
        DateTime? modify_time { get; set; }
        DateTime? modify_time_utc { get; set; }
        string? modify_user_id { get; set; }
        string? modify_user_name { get; set; }
        string? modify_log_id { get; set; }
    }


    public interface IMasterTableDto : IUpdatableDto
    {

    }

    public interface IRelationalTableDto : IVoidableDto
    {

    }

    public interface ITransactionDto : IUpdatableDto
    {

    }
}
