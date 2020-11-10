namespace Domain
{
    public interface IRepoConfig
    {
        bool Testing { get; }
        int DelaySecondsBeforePersisting { get; }
        int DelayMillisecondsBeforeVerify { get; }
    }

    public class RepoConfig : IRepoConfig
    {
        public bool Testing => false;

        public int DelaySecondsBeforePersisting => 90;

        public int DelayMillisecondsBeforeVerify => 500;
    }
}