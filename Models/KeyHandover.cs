namespace KeyManagment.Models
{
    public class KeyHandover
    {
        public int Id { get; set; }
        public DateTime CheckoutTime { get; set; }    // زمان تحویل
        public DateTime? ReturnTime { get; set; }     // زمان بازگشت (null = هنوز پیشش)
        public string? Notes { get; set; }            // توضیحات اضافه

        // کلیدی که تحویل داده شده
        public int KeyId { get; set; }
        public Key Key { get; set; } = null!;

        // کسی که کلید رو گرفته
        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverDepartment { get; set; } = string.Empty;

        // حراستی که تحویل داده
        public string GuardId { get; set; } = string.Empty;
        public string GuardName { get; set; } = string.Empty;
    }
}