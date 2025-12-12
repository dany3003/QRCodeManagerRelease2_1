
namespace QRCodeManagerRelease2.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendQRCodeFirstAccessNotificationAsync(string userEmail, string qrCode);
        Task SendRegistrationNotificationAsync(string userEmail, string firstName, string lastName, 
            bool isAzienda, string? nomeAzienda, string via, string? partitaIva, string? telefono, string? note);
        Task SendPasswordResetEmailAsync(string userEmail, string resetLink);

    }
}
