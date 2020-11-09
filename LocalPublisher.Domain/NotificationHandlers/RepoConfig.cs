namespace Domain
{
    public interface IRepoConfig
    {
        bool Testing { get; }
        int DelaySecondsBeforePersisting { get; }
    }

    public class RepoConfig : IRepoConfig
    {
        public bool Testing => false;

        public int DelaySecondsBeforePersisting => 90;
    }
}