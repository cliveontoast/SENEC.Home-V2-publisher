namespace Repository
{
    public interface ILocalContextConfiguration
    {
        string AccountEndPoint { get; }
        string AccountKey { get; }
        string DatabaseName { get; }
        string DefaultContainer { get; set; }
    }

    public class LocalContextConfiguration : ILocalContextConfiguration
    {
        public string AccountEndPoint { get; set; }
        public string AccountKey { get; set; }
        public string DatabaseName { get; set; }
        public string DefaultContainer { get; set; }

        public LocalContextConfiguration(string accountEndPoint, string accountKey, string databaseName, string defaultContainer)
        {
            AccountEndPoint = accountEndPoint;
            AccountKey = accountKey;
            DatabaseName = databaseName;
            DefaultContainer = defaultContainer;
        }
    }
}