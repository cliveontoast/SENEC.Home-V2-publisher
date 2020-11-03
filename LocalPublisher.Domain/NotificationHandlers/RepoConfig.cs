namespace Domain
{
    public interface IRepoConfig
    {
        bool Testing { get; }
    }

    public class RepoConfig : IRepoConfig
    {
        public bool Testing => false;
    }
}