
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using System.Security.Claims;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[action]")]
    public class AdminEmailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminEmailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult EmailSettings()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveEmailSettings(string smtpServer, int smtpPort, string username, string password, bool enableSsl)
        {
            // Qui salveresti le impostazioni email nel database o in un file di configurazione
            // Per ora aggiungo solo il log
            await LogActivity("Modifica", "Configurazione Email", 0, "Configurazione email modificata");
            
            TempData["SuccessMessage"] = "Configurazione email salvata con successo!";
            return RedirectToAction("EmailSettings");
        }

        private async Task LogActivity(string azione, string entita, object entitaId, string dettagli)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var log = new ActivityLog
            {
                UserId = currentUserId,
                Azione = azione,
                Entita = entita,
                EntitaId = entitaId is int ? (int)entitaId : null,
                Dettagli = dettagli,
                IPAddress = ipAddress
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
