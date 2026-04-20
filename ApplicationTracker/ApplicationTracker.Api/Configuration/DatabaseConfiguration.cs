namespace ApplicationTracker.Api.Configuration
{
    public class DatabaseConfiguration
    {
        public DatabaseConfiguration(string appDbContext)
        {
            AppDbContext = appDbContext;
        }

        public string AppDbContext { get; set; }
    }
}