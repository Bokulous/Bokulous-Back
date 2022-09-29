namespace Bokulous_Back.Services
{
    public interface IBokulousMailService
    {
        void SendEmail(string ToAddress, string subject, string body);
    }
}