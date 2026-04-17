namespace OnlineBakeshop.API.Model
{
    // Used to receive the refresh token request from frontend
    public class RefreshTokenModel
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    // Used to receive the revoke/logout request
    public class RevokeTokenModel
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}