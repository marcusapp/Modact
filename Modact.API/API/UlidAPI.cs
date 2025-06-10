using System.Text;

namespace Modact.API
{
    internal class UlidAPI
    {
        public UlidAPI()
        {

        }
        public void RegisterAPI(WebApplication app)
        {
            app.MapGet("/ulid", () =>
            {
                return Results.Ok(Ulid.NewUlid().ToString());
            });
            app.MapGet("/ulid/{count:int}", (int count) =>
            {
                var ulids = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    ulids.Add(Ulid.NewUlid().ToString());
                }
                return Results.Ok(ulids);
            });
        }
    }
}