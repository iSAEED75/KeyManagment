namespace KeyManagment.Models
{
    public class KeyHandover
    {
        public int Id { get; set; }
        public DateTime CheckoutTime { get; set; }
        public DateTime? ReturnTime { get; set; }
        public string? Notes { get; set; }
        public string? ReturnNotes { get; set; }


        // مدت زمان مجاز (بر حسب ساعت)
        public double AllowedHours { get; set; } = 8;

        // زمان انقضا
        public DateTime? ExpiryTime =>
            CheckoutTime.AddHours(AllowedHours);

        // آیا منقضی شده؟
        public bool IsExpired =>
            ReturnTime == null && DateTime.Now > ExpiryTime;

        public int KeyId { get; set; }
        public Key Key { get; set; } = null!;

        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverDepartment { get; set; }
        public string GuardId { get; set; } = string.Empty;
        public string GuardName { get; set; } = string.Empty;
    }
}