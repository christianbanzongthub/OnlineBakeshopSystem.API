using OnlineBakeshop.API.Model.Request;
using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IDeviceRepository
    {
        Task<ServiceResponse<object>> RegisterDeviceAsync(int userId, RegisterDeviceRequest request);
    }
}