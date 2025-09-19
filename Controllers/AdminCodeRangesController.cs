
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;
using QRCodeManagerRelease2.Services;
using System.Security.Claims;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[action]")]
    public class AdminCodeRangesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public AdminCodeRangesController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        public async Task<IActionResult> CodeRanges()
        {
            var ranges = await _context.CodeRanges
                .Include(cr => cr.Company)
                .ToListAsync();
                
            ViewBag.Companies = await _context.Companies.ToListAsync();
            
            return View(ranges);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCodeRange(CodeRangeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var range = new CodeRange
                {
                    CodiceIniziale = model.CodiceIniziale,
                    CodiceFinale = model.CodiceFinale,
                    CompanyId = model.CompanyId
                };

                _context.CodeRanges.Add(range);
                await _context.SaveChangesAsync();
                
                await LogActivity("Creazione", "Range Codici", range.Id, $"Range {range.CodiceIniziale}-{range.CodiceFinale} creato");
                
                TempData["SuccessMessage"] = "Range codici creato con successo!";
            }
            return RedirectToAction("CodeRanges");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCodeRange(int id, string codiceIniziale, string codiceFinale, int? companyId)
        {
            var range = await _context.CodeRanges.FindAsync(id);
            if (range != null)
            {
                range.CodiceIniziale = codiceIniziale;
                range.CodiceFinale = codiceFinale;
                range.CompanyId = companyId;

                await _context.SaveChangesAsync();
                
                await LogActivity("Modifica", "Range Codici", id, $"Range {range.CodiceIniziale}-{range.CodiceFinale} modificato");
                
                TempData["SuccessMessage"] = "Range codici modificato con successo!";
            }
            return RedirectToAction("CodeRanges");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCodeRange(int id)
        {
            var range = await _context.CodeRanges.FindAsync(id);
            if (range != null)
            {
                _context.CodeRanges.Remove(range);
                await _context.SaveChangesAsync();
                
                await LogActivity("Cancellazione", "Range Codici", id, $"Range {range.CodiceIniziale}-{range.CodiceFinale} eliminato");
                
                TempData["SuccessMessage"] = "Range codici eliminato con successo!";
            }
            return RedirectToAction("CodeRanges");
        }

        [HttpPost]
        public async Task<IActionResult> FindCodeRange(string searchCode)
        {
            if (string.IsNullOrEmpty(searchCode))
            {
                TempData["ErrorMessage"] = "Inserire un codice da cercare";
                return RedirectToAction("CodeRanges");
            }

            var foundRange = await _context.CodeRanges
                .Include(cr => cr.Company)
                .FirstOrDefaultAsync(cr => string.Compare(searchCode, cr.CodiceIniziale) >= 0 && 
                                          string.Compare(searchCode, cr.CodiceFinale) <= 0);

            if (foundRange != null)
            {
                TempData["SuccessMessage"] = $"Codice {searchCode} trovato nel range: {foundRange.CodiceIniziale} - {foundRange.CodiceFinale} ({foundRange.Company?.RagioneSociale ?? "Generale"})";
            }
            else
            {
                TempData["ErrorMessage"] = $"Codice {searchCode} non trovato in nessun range";
            }

            return RedirectToAction("CodeRanges");
        }

        [HttpGet]
        public async Task<IActionResult> ExportCodeRangesExcel()
        {
            var ranges = await _context.CodeRanges
                .Include(cr => cr.Company)
                .ToListAsync();

            var fileContents = await _exportService.ExportCodeRangesToExcel(ranges);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CodeRanges_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportCodeRangesPdf()
        {
            var ranges = await _context.CodeRanges
                .Include(cr => cr.Company)
                .ToListAsync();

            var fileContents = await _exportService.ExportCodeRangesToPdf(ranges);
            return File(fileContents, "application/pdf", $"CodeRanges_{DateTime.Now:yyyyMMdd}.pdf");
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
