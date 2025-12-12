
using System.Net;
using System.Net.Mail;

namespace QRCodeManagerRelease2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential("notifiche@sigiltech.it", "ooto jfaw rqbh zipf");
                
                var message = new MailMessage();
                message.From = new MailAddress("notifiche@sigiltech.it", "Sigiltech");
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log dell'errore ma non bloccare l'operazione
                Console.WriteLine($"Errore invio email: {ex.Message}");
            }
        }
        
        public async Task SendQRCodeFirstAccessNotificationAsync(string userEmail, string qrCode)
        {
            var subject = "Primo accesso al tuo QR Code";
            var body = $@"
                <h2>Notifica primo accesso</h2>
                <p>Il tuo QR Code <strong>{qrCode}</strong> è stato scansionato per la prima volta.</p>
                <p>Data e ora: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                <br>
                <p>Cordiali saluti,<br>Team SigiltecH</p>
            ";
            
            await SendEmailAsync(userEmail, subject, body);
        }


        public async Task SendPasswordResetEmailAsync(string userEmail, string resetLink)
        {
            var subject = "Reimposta la tua password - QRCodeManager";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #007bff; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0;'>Reimposta Password</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <p>Ciao,</p>
                        <p>Abbiamo ricevuto una richiesta per reimpostare la password del tuo account QRCodeManager.</p>
                        <p>Clicca sul pulsante qui sotto per creare una nuova password:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Reimposta Password
                            </a>
                        </div>
                        <p style='color: #666; font-size: 14px;'>Se non hai richiesto tu il reset della password, ignora questa email.</p>
                        <p style='color: #666; font-size: 14px;'>Questo link scadrà tra 24 ore.</p>
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                        <p style='color: #999; font-size: 12px;'>Se il pulsante non funziona, copia e incolla questo link nel tuo browser:</p>
                        <p style='color: #999; font-size: 12px; word-break: break-all;'>{resetLink}</p>
                    </div>
                    <div style='background-color: #333; color: white; padding: 15px; text-align: center; font-size: 12px;'>
                        <p style='margin: 0;'>© {DateTime.Now.Year} Sigiltech - Tutti i diritti riservati</p>
                    </div>
                </div>
            ";

            await SendEmailAsync(userEmail, subject, body);
        }

        public async Task SendRegistrationNotificationAsync(string userEmail, string firstName, string lastName, 
            bool isAzienda, string? nomeAzienda, string via, string? partitaIva, string? telefono, string? note)
        {
            var subject = "Nuova registrazione utente - QRCodeManager";
            var tipoUtente = isAzienda ? "Azienda" : "Privato";
            
            var body = $@"
                <h2>Nuova registrazione utente</h2>
                <p>Un nuovo utente si è registrato al sistema QRCodeManager:</p>
                <br>
                <table style='border-collapse: collapse; width: 100%;'>
                    <tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Nome:</td><td style='border: 1px solid #ddd; padding: 8px;'>{firstName}</td></tr>
                    <tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Cognome:</td><td style='border: 1px solid #ddd; padding: 8px;'>{lastName}</td></tr>
                    <tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Email:</td><td style='border: 1px solid #ddd; padding: 8px;'>{userEmail}</td></tr>
                    <tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Tipo:</td><td style='border: 1px solid #ddd; padding: 8px;'>{tipoUtente}</td></tr>
                    {(isAzienda ? $"<tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Nome Azienda:</td><td style='border: 1px solid #ddd; padding: 8px;'>{nomeAzienda}</td></tr>" : "")}
                    <tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Indirizzo:</td><td style='border: 1px solid #ddd; padding: 8px;'>{via}</td></tr>
                    {(isAzienda && !string.IsNullOrEmpty(partitaIva) ? $"<tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Partita IVA:</td><td style='border: 1px solid #ddd; padding: 8px;'>{partitaIva}</td></tr>" : "")}
                    {(!string.IsNullOrEmpty(telefono) ? $"<tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Telefono:</td><td style='border: 1px solid #ddd; padding: 8px;'>{telefono}</td></tr>" : "")}
                    {(!string.IsNullOrEmpty(note) ? $"<tr><td style='border: 1px solid #ddd; padding: 8px; font-weight: bold;'>Note:</td><td style='border: 1px solid #ddd; padding: 8px;'>{note}</td></tr>" : "")}
                </table>
                <br>
                <p><strong>L'utente è in attesa di abilitazione.</strong></p>
                <br>
                <p>Cordiali saluti,<br>Sistema QR-Link</p>
            ";

            // Invia a entrambi gli indirizzi
            await SendEmailAsync("notifiche@sigiltech.it", subject, body);
            await SendEmailAsync("info@sigiltech.it", subject, body);
            await SendEmailAsync("claudia.tailli@sigiltech.it", subject, body);
        }
    }
}
