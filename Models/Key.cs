namespace KeyManagment.Models
{
    public class Key
    {
        public int Id { get; set; }
        public string KeyCode { get; set; } = string.Empty;  // مثلاً B1-F2-R104
        public string RoomName { get; set; } = string.Empty; // اسم اتاق
        public int Floor { get; set; }                        // طبقه
        public bool IsAvailable { get; set; } = true;        // آیا موجوده؟

        public int BuildingId { get; set; }
        public Building Building { get; set; } = null!;

        public ICollection<KeyHandover> Handovers { get; set; } = new List<KeyHandover>();
    }
}