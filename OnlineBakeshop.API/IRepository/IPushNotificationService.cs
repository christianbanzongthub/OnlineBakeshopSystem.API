using OnlineBakeshop.API.Model.Request;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IPushNotificationService
    {
        Task<ServiceResponse<object>> SendToUsersAsync(
            SendPushNotificationRequest request,
            CancellationToken cancellationToken = default);
    }
}