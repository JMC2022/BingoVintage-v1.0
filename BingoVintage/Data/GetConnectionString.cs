namespace BingoVintage.Data
{
    public class GetConnectionString
    {
        //Method to get configuration of db.
        public string ConnectionString(string db)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
            IConfiguration configuration = builder.Build();
            return configuration.GetConnectionString(db);
        }
    }
}