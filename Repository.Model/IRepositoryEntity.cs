namespace Repository.Model
{
    public interface IRepositoryEntity
    {
        string Key { get; set; }
        string Partition { get; set; }
        int Version { get; set; }
    }
}