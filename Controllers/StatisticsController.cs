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

        public async Task<IActionResult> Index(string? userId = null)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser?.CompanyId == null)
            {
                return RedirectToAction("Dashboard", "Home");
            }


            // Recupera utenti del gruppo/azienda
            var groupUsers = await _userManager.Users
                .Where(u => u.CompanyId == currentUser.CompanyId ) //&& u.IsApproved
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            var stats = new UserStatisticsViewModel
            {
                User = currentUser,
                Company = currentUser.Company,
                SelectedUserId = userId,
                GroupUsers = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = "", Text = "Tutti gli utenti" }
                }
            };

            // Popola dropdown utenti
            foreach (var user in groupUsers)
            {
                stats.GroupUsers.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = user.Id,
                    Text = $"{user.LastName} {user.FirstName}",
                    Selected = user.Id == userId
                });
            }

            // Query QR Codes in base al filtro utente
            IQueryable<QRCode> qrCodesQuery = _context.QRCodes
                .Include(q => q.CreatedBy)
                .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == currentUser.CompanyId);

            


            var qrCodes = await qrCodesQuery.ToListAsync();
            if (!string.IsNullOrEmpty(userId))
            {
             var QRCodess = await qrCodesQuery.Where(q => q.UserId == userId).ToListAsync();
            stats.QRCodes = QRCodess;
            }
            else {

                stats.QRCodes = qrCodes;
            }


                
            stats.TotalQRCodes = qrCodes.Count;
            stats.UsedQRCodes = qrCodes.Count(q => q.NumeroChiamate > 0);
            stats.TotalCalls = qrCodes.Sum(q => q.NumeroChiamate);
            stats.AverageCalls = qrCodes.Count > 0 ? (decimal)stats.TotalCalls / qrCodes.Count : 0;

            // Statistiche personali
            stats.PersonalQRCodes = qrCodes.Count(q => q.UserId == currentUserId);
            stats.PersonalUsedQRCodes = qrCodes.Count(q => q.UserId == currentUserId && q.NumeroChiamate > 0);
            stats.CompanyQRCodes = qrCodes.Count;
            stats.CompanyUsedQRCodes = qrCodes.Count(q => q.NumeroChiamate > 0);

            await LogActivity("Visualizzazione", "Statistiche", 0, $"Utente ha visualizzato le statistiche{(userId != null ? $" filtrato per utente {userId}" : "")}");

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
