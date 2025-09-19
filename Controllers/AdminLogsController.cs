
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.Services;
using System.Security.Claims;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[action]")]
    public class AdminLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public AdminLogsController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        public async Task<IActionResult> ActivityLogs()
        {
            var logs = await _context.ActivityLogs
                .Include(al => al.User)
                .OrderByDescending(al => al.DataOperazione)
                .ToListAsync();
                
            ViewBag.Users = await _context.Users.ToListAsync();
            
            return View(logs);
        }

        public async Task<IActionResult> Anomalies()
        {
            var anomalies = await _context.Anomalies
                .Include(a => a.User)
                .Include(a => a.RisoltaDa)
                .OrderByDescending(a => a.DataSegnalazione)
                .ToListAsync();
            return View(anomalies);
        }

        public async Task<IActionResult> AnomalyDetails(int id)
        {
            var anomaly = await _context.Anomalies
                .Include(a => a.User)
                    .ThenInclude(u => u.Company)
                .Include(a => a.User)
                    .ThenInclude(u => u.CustomerGroup)
                .Include(a => a.RisoltaDa)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anomaly == null) return NotFound();

            return View(anomaly);
        }

        [HttpPost]
        public async Task<IActionResult> ResolveAnomaly(int anomalyId)
        {
            var anomaly = await _context.Anomalies.FindAsync(anomalyId);
            if (anomaly != null)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                anomaly.Risolta = true;
                anomaly.DataRisoluzione = DateTime.Now;
                anomaly.RisoltaDaUserId = currentUserId;

                await _context.SaveChangesAsync();
                await LogActivity("Risoluzione", "Anomalia", anomalyId, "Anomalia risolta");
                
                TempData["SuccessMessage"] = "Anomalia risolta!";
            }
            return RedirectToAction("Anomalies");
        }

        [HttpGet]
        public async Task<IActionResult> ExportActivityLogsExcel()
        {
            var logs = await _context.ActivityLogs
                .Include(al => al.User)
                .OrderByDescending(al => al.DataOperazione)
                .ToListAsync();

            var fileContents = await _exportService.ExportActivityLogsToExcel(logs);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ActivityLogs_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportActivityLogsPdf()
        {
            var logs = await _context.ActivityLogs
                .Include(al => al.User)
                .OrderByDescending(al => al.DataOperazione)
                .ToListAsync();

            var fileContents = await _exportService.ExportActivityLogsToPdf(logs);
            return File(fileContents, "application/pdf", $"ActivityLogs_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportAnomaliesExcel()
        {
            var anomalies = await _context.Anomalies
                .Include(a => a.User)
                .Include(a => a.RisoltaDa)
                .OrderByDescending(a => a.DataSegnalazione)
                .ToListAsync();

            var fileContents = await _exportService.ExportAnomaliesToExcel(anomalies);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Anomalies_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportAnomaliesPdf()
        {
            var anomalies = await _context.Anomalies
                .Include(a => a.User)
                .Include(a => a.RisoltaDa)
                .OrderByDescending(a => a.DataSegnalazione)
                .ToListAsync();

            var fileContents = await _exportService.ExportAnomaliesToPdf(anomalies);
            return File(fileContents, "application/pdf", $"Anomalies_{DateTime.Now:yyyyMMdd}.pdf");
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
