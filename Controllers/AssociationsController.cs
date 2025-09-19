using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Services;
using System.Security.Claims;
using System.Drawing;
using QRCoder;
using System.Drawing.Imaging;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize]
    public class AssociationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public AssociationsController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        public async Task<IActionResult> Index(string sortBy = "CreatedAt", string sortDir = "desc", string search = "")
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.QRCodes
                .Include(q => q.CreatedBy)
                .Include(q => q.CallHistory)
                .Where(q => q.CreatedBy.Id == currentUserId);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => q.Content.Contains(search) ||
                                        (q.DestinationLink != null && q.DestinationLink.Contains(search)) ||
                                        (q.CodiceUtilizzo != null && q.CodiceUtilizzo.Contains(search)));
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "content" => sortDir == "asc" ? query.OrderBy(q => q.Content) : query.OrderByDescending(q => q.Content),
                "destination" => sortDir == "asc" ? query.OrderBy(q => q.DestinationLink) : query.OrderByDescending(q => q.DestinationLink),
                "calls" => sortDir == "asc" ? query.OrderBy(q => q.NumeroChiamate) : query.OrderByDescending(q => q.NumeroChiamate),
                "lastcall" => sortDir == "asc" ? query.OrderBy(q => q.UltimaChiamata) : query.OrderByDescending(q => q.UltimaChiamata),
                _ => sortDir == "asc" ? query.OrderBy(q => q.CreatedAt) : query.OrderByDescending(q => q.CreatedAt)
            };

            var qrCodes = await query.ToListAsync();

            ViewBag.SortBy = sortBy;
            ViewBag.SortDir = sortDir;
            ViewBag.Search = search;

            return View(qrCodes);
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

        [HttpPost]
        public async Task<IActionResult> DisableQRCode(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var qrCode = await _context.QRCodes
                .FirstOrDefaultAsync(q => q.Id == id && q.CreatedBy.Id == currentUserId);

            if (qrCode != null)
            {
                qrCode.Bloccato = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "QR Code disabilitato con successo!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string sortBy = "CreatedAt", string sortDir = "desc", string search = "")
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.QRCodes
                .Include(q => q.CreatedBy)
                .Include(q => q.CallHistory)
                .Where(q => q.CreatedBy.Id == currentUserId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => q.Content.Contains(search) ||
                                        (q.DestinationLink != null && q.DestinationLink.Contains(search)) ||
                                        (q.CodiceUtilizzo != null && q.CodiceUtilizzo.Contains(search)));
            }

            query = sortBy.ToLower() switch
            {
                "content" => sortDir == "asc" ? query.OrderBy(q => q.Content) : query.OrderByDescending(q => q.Content),
                "destination" => sortDir == "asc" ? query.OrderBy(q => q.DestinationLink) : query.OrderByDescending(q => q.DestinationLink),
                "calls" => sortDir == "asc" ? query.OrderBy(q => q.NumeroChiamate) : query.OrderByDescending(q => q.NumeroChiamate),
                "lastcall" => sortDir == "asc" ? query.OrderBy(q => q.UltimaChiamata) : query.OrderByDescending(q => q.UltimaChiamata),
                _ => sortDir == "asc" ? query.OrderBy(q => q.CreatedAt) : query.OrderByDescending(q => q.CreatedAt)
            };

            var qrCodes = await query.ToListAsync();
            var fileContents = await _exportService.ExportQRCodesToExcel(qrCodes);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MieAssociazioni_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(string sortBy = "CreatedAt", string sortDir = "desc", string search = "")
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.QRCodes
                .Include(q => q.CreatedBy)
                .Include(q => q.CallHistory)
                .Where(q => q.CreatedBy.Id == currentUserId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => q.Content.Contains(search) ||
                                        (q.DestinationLink != null && q.DestinationLink.Contains(search)) ||
                                        (q.CodiceUtilizzo != null && q.CodiceUtilizzo.Contains(search)));
            }

            query = sortBy.ToLower() switch
            {
                "content" => sortDir == "asc" ? query.OrderBy(q => q.Content) : query.OrderByDescending(q => q.Content),
                "destination" => sortDir == "asc" ? query.OrderBy(q => q.DestinationLink) : query.OrderByDescending(q => q.DestinationLink),
                "calls" => sortDir == "asc" ? query.OrderBy(q => q.NumeroChiamate) : query.OrderByDescending(q => q.NumeroChiamate),
                "lastcall" => sortDir == "asc" ? query.OrderBy(q => q.UltimaChiamata) : query.OrderByDescending(q => q.UltimaChiamata),
                _ => sortDir == "asc" ? query.OrderBy(q => q.CreatedAt) : query.OrderByDescending(q => q.CreatedAt)
            };

            var qrCodes = await query.ToListAsync();
            var fileContents = await _exportService.ExportQRCodesToPdf(qrCodes);
            return File(fileContents, "application/pdf", $"MieAssociazioni_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadQRCode(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var qrCode = await _context.QRCodes
                .FirstOrDefaultAsync(q => q.Id == id && q.CreatedBy.Id == currentUserId);

            if (qrCode == null)
            {
                return NotFound();
            }

            try
            {

                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(qrCode.Content, QRCodeGenerator.ECCLevel.Q);

                    var qrCodeImage = new PngByteQRCode(qrCodeData);
                    byte[] imageBytes = qrCodeImage.GetGraphic(20); // <-- Questo è già un PNG!

                    return File(imageBytes, "image/png", $"QRCode_{qrCode.ExtractedCode}.png");
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Errore nella generazione del QR Code";
                return RedirectToAction("Index");
            }
        }


    }
}