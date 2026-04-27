using System.ComponentModel.DataAnnotations;

namespace OnlineBakeshop.API.Model.Request
{
    public class RegisterDeviceRequest
    {
        [Required]
        [MaxLength(512)]
        public string FcmToken { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Platform { get; set; } = string.Empty; // Android | iOS | Web

        [MaxLength(128)]
        public string? DeviceIdentifier { get; set; }

        [MaxLength(256)]
        public string? DeviceName { get; set; }
    }
}