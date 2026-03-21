namespace OnlineBakeshop.API.Model
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }


    }

    public class RegisterModel
    {

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
    }


}
