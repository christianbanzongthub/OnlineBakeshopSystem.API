using OnlineBakeshop.API.Model.Request;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IPushNotificationService
    {
        Task<ServiceResponse<object>> SendToUsersAsync(
            SendPushNotificationRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<object>> SendFromUserAsync(
            int senderUserId,
            SendUserNotificationRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<object>> GetMyNotificationsAsync(
            int userId,
            int page,
            int pageSize);

        Task<ServiceResponse<object>> MarkAsReadAsync(
            int userId,
            long notificationId);

        Task<ServiceResponse<object>> MarkAllAsReadAsync(
            int userId);

        Task<ServiceResponse<object>> DeleteForUserAsync(
            int userId,
            long notificationId);

        Task<ServiceResponse<object>> GetMyNotificationByIdAsync(
            int userId,
            long notificationId);
    }
    
}