namespace Shared
{
    public interface IApplicationVersion
    {
        int Number { get; }
        int? PersistedNumber { get; set; }
        bool ThisProcessWrittenRecord { get; set; }
    }

    public class ApplicationVersion : IApplicationVersion
    {
        public int Number { get => 1; }
        public int? PersistedNumber { get; set; }
        public bool ThisProcessWrittenRecord { get; set; }
    }
}