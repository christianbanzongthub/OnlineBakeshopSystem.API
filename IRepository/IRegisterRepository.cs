using OnlineBakeshop.API.Model.Response;

namespace OnlineBakeshop.API.IRepository
{
    public interface IRegisterRepository
    {
        Task<ServiceResponse<object>> GetRegister(string fullname, string email, string password, string address, string contactNo);
    }
}