namespace OnlineBakeshop.API.Model.Response
{
    public class UserNotificationDto
    {
        public long NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? DataJson { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public int? SenderUserId { get; set; }
    }
}