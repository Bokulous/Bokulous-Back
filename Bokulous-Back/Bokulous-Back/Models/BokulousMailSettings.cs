namespace Bokulous_Back.Models
{
    public class BokulousMailSettings
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "";
        public string SmptAddress { get; set; } = "";
        public string Port { get; set; } = "";
    }
}