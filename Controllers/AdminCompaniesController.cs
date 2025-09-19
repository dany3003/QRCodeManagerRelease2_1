using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;
using QRCodeManagerRelease2.Services;
using System.Security.Claims;
using System.IO;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[action]")]
    public class AdminCompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExportService _exportService;

        public AdminCompaniesController(ApplicationDbContext context, IExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies.ToListAsync();
            return View(companies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var company = new Company
                {
                    RagioneSociale = model.RagioneSociale,
                    Codice = model.Codice,
                    Descrizione = model.Descrizione,
                    Via = model.Via,
                    PartitaIva = model.PartitaIva
                };

                // Handle logo upload
                if (model.Logo != null && model.Logo.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.Logo.FileName).ToLowerInvariant();
                    
                    if (allowedExtensions.Contains(extension))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await model.Logo.CopyToAsync(memoryStream);
                            var fileBytes = memoryStream.ToArray();
                            var base64String = Convert.ToBase64String(fileBytes);
                            company.LogoPath = $"data:{model.Logo.ContentType};base64,{base64String}";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Formato file non supportato. Utilizzare jpg, jpeg, png o gif.";
                        return RedirectToAction("Companies");
                    }
                }

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
                
                await LogActivity("Creazione", "Azienda", company.Id, $"Azienda {company.RagioneSociale} creata");
                
                TempData["SuccessMessage"] = "Azienda creata con successo!";
            }
            return RedirectToAction("Companies");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCompany(int id, string ragioneSociale, string codice, string descrizione, IFormFile? logo, string via, string partitaIva)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                company.RagioneSociale = ragioneSociale;
                company.Codice = codice;
                company.Descrizione = descrizione;
                company.Via = via;
                company.PartitaIva = partitaIva;

                // Handle logo upload
                if (logo != null && logo.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
                    
                    if (allowedExtensions.Contains(extension))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await logo.CopyToAsync(memoryStream);
                            var fileBytes = memoryStream.ToArray();
                            var base64String = Convert.ToBase64String(fileBytes);
                            company.LogoPath = $"data:{logo.ContentType};base64,{base64String}";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Formato file non supportato. Utilizzare jpg, jpeg, png o gif.";
                        return RedirectToAction("Companies");
                    }
                }

                await _context.SaveChangesAsync();
                
                await LogActivity("Modifica", "Azienda", id, $"Azienda {company.RagioneSociale} modificata");
                
                TempData["SuccessMessage"] = "Azienda modificata con successo!";
            }
            return RedirectToAction("Companies");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
                
                await LogActivity("Cancellazione", "Azienda", id, $"Azienda {company.RagioneSociale} eliminata");
                
                TempData["SuccessMessage"] = "Azienda eliminata con successo!";
            }
            return RedirectToAction("Companies");
        }

        [HttpGet]
        public async Task<IActionResult> ExportCompaniesExcel()
        {
            var companies = await _context.Companies.ToListAsync();
            var fileContents = await _exportService.ExportCompaniesToExcel(companies);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Aziende_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportCompaniesPdf()
        {
            var companies = await _context.Companies.ToListAsync();
            var fileContents = await _exportService.ExportCompaniesToPdf(companies);
            return File(fileContents, "application/pdf", $"Aziende_{DateTime.Now:yyyyMMdd}.pdf");
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
