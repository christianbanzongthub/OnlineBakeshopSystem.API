using System.ComponentModel.DataAnnotations;

namespace OnlineBakeshop.API.Model.Request
{
    public class SendPushNotificationRequest
    {
        [Required]
        [MinLength(1)]
        public List<int> TargetUserIds { get; set; } = new();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Body { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? Type { get; set; }

        public Dictionary<string, string>? Data { get; set; }
    }
}