namespace POSAPI.Config
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = "POSAPI";
        public string Audience { get; set; } = "POS.UI";
        public string SecretKey { get; set; } = "";
        public int ExpiryMinutes { get; set; } = 480;
    }
}
