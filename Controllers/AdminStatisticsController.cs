
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;
using QRCodeManagerRelease2.Services;
using System.Drawing;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[action]")]
    public class AdminStatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public AdminStatisticsController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        public async Task<IActionResult> Statistics()
        {
            var companies = await _context.Companies
                .Include(c => c.Users)
                .Include(c => c.CodeRanges)
                .ToListAsync();

            var stats = new List<CompanyStatViewModel>();
            
            foreach (var company in companies)
            {
                var totalQRCodes = await _context.QRCodes
                    .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id)
                    .CountAsync();

                var usedQRCodes = await _context.QRCodes
                    .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id && q.NumeroChiamate > 0)
                    .CountAsync();

                stats.Add(new CompanyStatViewModel
                {
                    Company = company,
                    TotalQRCodes = totalQRCodes,
                    UsedQRCodes = usedQRCodes
                });
            }

            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyContacts(int companyId)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound();
            }

            var contacts = company.Users.Select(u => new
            {
                Nome = u.FirstName,
                Cognome = u.LastName,
                Email = u.Email,
                Telefono = u.PhoneNumber ?? "N/A"
            }).ToList();

            return Json(contacts);
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyDetails(int companyId)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .Include(c => c.CodeRanges)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound();
            }

            var totalQRCodes = await _context.QRCodes
                .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id)
                .CountAsync();

            var usedQRCodes = await _context.QRCodes
                .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id && q.NumeroChiamate > 0)
                .CountAsync();

            var details = new
            {

                RagioneSociale =  company.RagioneSociale,
                Codice = company.Codice,
                Via = company.Via,
                PartitaIva = company.PartitaIva,
                TotalQRCodes = totalQRCodes,
                UsedQRCodes = usedQRCodes,
                RemainingCodes = totalQRCodes - usedQRCodes,
                Users = company.Users.Count,
                CodeRanges = company.CodeRanges.Count
            };

            return Json(details);
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticsExcel()
        {
            var companies = await _context.Companies
                .Include(c => c.Users)
                .Include(c => c.CodeRanges)
                .ToListAsync();

            var stats = new List<CompanyStatViewModel>();
            
            foreach (var company in companies)
            {
                var totalQRCodes = await _context.QRCodes
                    .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id)
                    .CountAsync();

                var usedQRCodes = await _context.QRCodes
                    .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id && q.NumeroChiamate > 0)
                    .CountAsync();

                stats.Add(new CompanyStatViewModel
                {
                    Company = company,
                    TotalQRCodes = totalQRCodes,
                    UsedQRCodes = usedQRCodes
                });
            }

            var fileContents = await _exportService.ExportCompanyStatisticsToExcel(stats);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"StatisticsConsumi_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatisticsPdf()
        {
            var companies = await _context.Companies
                .Include(c => c.Users)
                .Include(c => c.CodeRanges)
                .ToListAsync();

            var stats = new List<CompanyStatViewModel>();
            
            foreach (var company in companies)
            {
                var totalQRCodes = await _context.QRCodes
                    .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id)
                    .CountAsync();

                var usedQRCodes = await _context.QRCodes
                    .Where(q => q.CreatedBy != null && q.CreatedBy.CompanyId == company.Id && q.NumeroChiamate > 0)
                    .CountAsync();

                stats.Add(new CompanyStatViewModel
                {
                    Company = company,
                    TotalQRCodes = totalQRCodes,
                    UsedQRCodes = usedQRCodes
                });
            }

            var fileContents = await _exportService.ExportCompanyStatisticsToPdf(stats);
            return File(fileContents, "application/pdf", $"StatisticsConsumi_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}
