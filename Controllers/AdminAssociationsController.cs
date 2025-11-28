
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
    public class AdminAssociationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public AdminAssociationsController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        public async Task<IActionResult> Associations(string userFilter = "", string dateFilter = "", string statusFilter = "", string sortBy = "CreatedAt", string sortDir = "desc")
        {
            var query = _context.QRCodes
                .Include(q => q.CreatedBy)
                .Include(q => q.CallHistory)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(userFilter))
            {
                query = query.Where(q => q.CreatedBy != null && q.CreatedBy.Email.Contains(userFilter));
            }

            if (!string.IsNullOrEmpty(dateFilter) && DateTime.TryParse(dateFilter, out var filterDate))
            {
                query = query.Where(q => q.CreatedAt.Date == filterDate.Date);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "active")
                    query = query.Where(q => !q.Bloccato);
                else if (statusFilter == "blocked")
                    query = query.Where(q => q.Bloccato);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "content" => sortDir == "asc" ? query.OrderBy(q => q.Content) : query.OrderByDescending(q => q.Content),
                "user" => sortDir == "asc" ? query.OrderBy(q => q.CreatedBy.Email) : query.OrderByDescending(q => q.CreatedBy.Email),
                "calls" => sortDir == "asc" ? query.OrderBy(q => q.NumeroChiamate) : query.OrderByDescending(q => q.NumeroChiamate),
                "lastcall" => sortDir == "asc" ? query.OrderBy(q => q.UltimaChiamata) : query.OrderByDescending(q => q.UltimaChiamata),
                _ => sortDir == "asc" ? query.OrderBy(q => q.CreatedAt) : query.OrderByDescending(q => q.CreatedAt)
            };

            var qrCodes = await query.ToListAsync();
            
            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.UserFilter = userFilter;
            ViewBag.DateFilter = dateFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            
            return View(qrCodes);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQRCode(int id, string destinationLink, string codiceUtilizzo, bool notificaPrimaApertura, string details, bool usePasswoword)
        {
            var qrCode = await _context.QRCodes.FindAsync(id);
            if (qrCode != null)
            {
                qrCode.UsePassword = usePasswoword;
                qrCode.Details = details;
                qrCode.DestinationLink = destinationLink;
                qrCode.CodiceUtilizzo = codiceUtilizzo;
                qrCode.AvvisamiPrimaVolta = notificaPrimaApertura;
                await _context.SaveChangesAsync();
                
                await LogActivity("Modifica", "QRCode", id, $"QR Code {qrCode.Content} modificato");
                
                TempData["SuccessMessage"] = "QR Code modificato con successo!";
            }
            return RedirectToAction("Associations");
        }

        [HttpPost]
        public async Task<IActionResult> BlockQRCode(int id)
        {
            var qrCode = await _context.QRCodes.FindAsync(id);
            if (qrCode != null)
            {
                qrCode.Bloccato = true;
                await _context.SaveChangesAsync();
                
                await LogActivity("Blocco", "QRCode", id, $"QR Code {qrCode.Content} bloccato");
                
                TempData["SuccessMessage"] = "QR Code bloccato con successo!";
            }
            return RedirectToAction("Associations");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockQRCode(int id)
        {
            var qrCode = await _context.QRCodes.FindAsync(id);
            if (qrCode != null)
            {
                qrCode.Bloccato = false;
                await _context.SaveChangesAsync();
                
                await LogActivity("Sblocco", "QRCode", id, $"QR Code {qrCode.Content} sbloccato");
                
                TempData["SuccessMessage"] = "QR Code sbloccato con successo!";
            }
            return RedirectToAction("Associations");
        }

        [HttpGet]
        public async Task<IActionResult> ExportAssociationsExcel(string userFilter = "", string dateFilter = "", string statusFilter = "")
        {
            var query = _context.QRCodes
                .Include(q => q.CreatedBy)
                .Include(q => q.CallHistory)
                .AsQueryable();

            // Apply same filters as the main view
            if (!string.IsNullOrEmpty(userFilter))
                query = query.Where(q => q.CreatedBy != null && q.CreatedBy.Email.Contains(userFilter));
            if (!string.IsNullOrEmpty(dateFilter) && DateTime.TryParse(dateFilter, out var filterDate))
                query = query.Where(q => q.CreatedAt.Date == filterDate.Date);
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "active") query = query.Where(q => !q.Bloccato);
                else if (statusFilter == "blocked") query = query.Where(q => q.Bloccato);
            }

            var qrCodes = await query.ToListAsync();
            var fileContents = await _exportService.ExportQRCodesToExcel(qrCodes);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Associazioni_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportAssociationsPdf(string userFilter = "", string dateFilter = "", string statusFilter = "")
        {
            var query = _context.QRCodes
                .Include(q => q.CreatedBy)
                .Include(q => q.CallHistory)
                .AsQueryable();

            // Apply same filters as the main view
            if (!string.IsNullOrEmpty(userFilter))
                query = query.Where(q => q.CreatedBy != null && q.CreatedBy.Email.Contains(userFilter));
            if (!string.IsNullOrEmpty(dateFilter) && DateTime.TryParse(dateFilter, out var filterDate))
                query = query.Where(q => q.CreatedAt.Date == filterDate.Date);
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "active") query = query.Where(q => !q.Bloccato);
                else if (statusFilter == "blocked") query = query.Where(q => q.Bloccato);
            }

            var qrCodes = await query.ToListAsync();
            var fileContents = await _exportService.ExportQRCodesToPdf(qrCodes);
            return File(fileContents, "application/pdf", $"Associazioni_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> FindCodeRange(string searchCode)
        {
            if (string.IsNullOrEmpty(searchCode))
            {
                TempData["ErrorMessage"] = "Inserire un codice per la ricerca";
                return RedirectToAction("Associations");
            }

            var codeRanges = await _context.CodeRanges
                .Include(cr => cr.Company)
                .Where(cr => string.Compare(searchCode, cr.CodiceIniziale) >= 0 && 
                            string.Compare(searchCode, cr.CodiceFinale) <= 0)
                .ToListAsync();

            if (codeRanges.Any())
            {
                var range = codeRanges.First();
                TempData["SuccessMessage"] = $"Codice {searchCode} trovato nel range: {range.CodiceIniziale} - {range.CodiceFinale} ({range.Company?.RagioneSociale ?? "Generale"})";
            }
            else
            {
                TempData["ErrorMessage"] = $"Codice {searchCode} non trovato in nessun range";
            }

            return RedirectToAction("Associations");
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

        public async Task<IActionResult> Details(int id)
        {
            var qrCode = await _context.QRCodes
                .Include(q => q.CreatedBy)
                .Include(q => q.CallHistory)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (qrCode == null)
            {
                return NotFound();
            }

            // Get code ranges for display
            var codeRanges = await _context.CodeRanges
                .Include(cr => cr.Company)
                .ToListAsync();
            ViewBag.CodeRanges = codeRanges;

            return View(qrCode);
        }

    }
}
