using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;
using System.Security.Claims;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser?.CompanyId == null)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var stats = new UserStatisticsViewModel
            {
                User = currentUser,
                Company = currentUser.Company
            };

            // QR Codes personali dell'utente
            var userQRCodes = await _context.QRCodes
                .Where(q => q.CreatedBy.Id == currentUserId)
                .ToListAsync();

            stats.PersonalQRCodes = userQRCodes.Count;
            stats.PersonalUsedQRCodes = userQRCodes.Count(q => q.NumeroChiamate > 0);

            // QR Codes dell'azienda
            var companyQRCodes = await _context.QRCodes
                .Include(q => q.CreatedBy)
                .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == currentUser.CompanyId)
                .ToListAsync();

            stats.CompanyQRCodes = companyQRCodes.Count;
            stats.CompanyUsedQRCodes = companyQRCodes.Count(q => q.NumeroChiamate > 0);

            // Dettagli per categoria
            stats.QRCodeDetails = userQRCodes.Select(q => new QRCodeDetailViewModel
            {
                QRCode = q,
                CreatedBy = currentUser,
                TotalCalls = q.NumeroChiamate,
                LastCall = q.CallHistory?.OrderByDescending(c => c.DataChiamata).FirstOrDefault()?.DataChiamata
            }).ToList();

            await LogActivity("Visualizzazione", "Statistiche", 0, "Utente ha visualizzato le statistiche");

            return View(stats);
        }

        public async Task<IActionResult> ExportExcel()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var userQRCodes = await _context.QRCodes
                .Where(q => q.CreatedBy.Id == currentUserId)
                .ToListAsync();

            // Per ora restituisco un messaggio - implementazione Excel richiede EPPlus
            TempData["ErrorMessage"] = "Funzionalità di export Excel in fase di implementazione";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ExportPDF()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            var userQRCodes = await _context.QRCodes
                .Where(q => q.CreatedBy.Id == currentUserId)
                .ToListAsync();

            // Per ora restituisco un messaggio - implementazione PDF richiede iTextSharp
            TempData["ErrorMessage"] = "Funzionalità di export PDF in fase di implementazione";
            return RedirectToAction("Index");
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
