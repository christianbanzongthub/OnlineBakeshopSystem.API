namespace OnlineBakeshop.API.Model
{
    public class AdminModel
    {
        public int AdminId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class AdminLoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}