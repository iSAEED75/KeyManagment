namespace KeyManagment.Models
{
    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<Key> Keys { get; set; } = new List<Key>();
    }
}