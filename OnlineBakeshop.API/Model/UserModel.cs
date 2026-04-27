namespace OnlineBakeshop.API.Model
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime DateCreated { get; set; }
    }
}