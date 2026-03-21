using OnlineBakeshop.API.Model.Response;
namespace OnlineBakeshop.API.IRepository
{
    public interface ILoginRepository
    {
        Task<ServiceResponse<object>> GetLogin(string email, string password);
        //public object GetLogin(string username, string password);
    }
}