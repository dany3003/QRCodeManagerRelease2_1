using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.Services;
using QRCodeManagerRelease2.ViewModels;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize]
    public class QRCodeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQRCodeService _qrCodeService;

        public QRCodeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IQRCodeService qrCodeService)
        {
            _context = context;
            _userManager = userManager;
            _qrCodeService = qrCodeService;
        }

        public IActionResult Assign(string content = "")
        {
            ViewData["Title"] = "Assegna QRCode";
            var model = new QRCodeViewModel();
            if (!string.IsNullOrEmpty(content))
            {
                model.Content = content;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(QRCodeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                Match match = Regex.Match(model.Content, @"s=(.+)");
                
                if (match.Success)
                {
                    model.ExtractedCode  = match.Groups[1].Value; // Il gruppo 1 è ciò che viene catturato tra parentesi
                }

               
                // Verifica se il codice è già utilizzato
                var existingCode = await _qrCodeService.GetQRCodeByContentAsync(model.ExtractedCode);

             
                

                if (existingCode != null)
                {
                    ModelState.AddModelError("Content", "Il codice selezionato, risulta già utilizzato");
                    return View(model);
                }

                // Verifica se il codice è nel range autorizzato
                bool isAuthorized = await IsCodeAuthorized(model.ExtractedCode, user);

                if (!isAuthorized && !User.IsInRole("Admin"))
                {
                    // Crea anomalia
                    var anomaly = new Anomaly
                    {
                        UserId = userId,
                        Tipo = "Codice non autorizzato",
                        Descrizione = $"L'utente {user?.Email} ha tentato di associare il codice {model.Content} non presente nei range autorizzati",
                        CodiceInteressato = model.ExtractedCode
                    };

                    _context.Anomalies.Add(anomaly);
                    await _context.SaveChangesAsync();

                    TempData["WarningMessage"] = "ATTENZIONE: Il codice inserito non è presente nei range autorizzati. La richiesta è stata segnalata agli amministratori.";
                }

                await _qrCodeService.CreateQRCodeAsync(model, userId);

                // Log attività
                await LogActivity("Creazione", "QRCode", model.ExtractedCode, $"QR Code {model.ExtractedCode} associato");

                TempData["SuccessMessage"] = "QR Code salvato con successo!";
                return RedirectToAction("Assign");
            }
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Access()
        {
            var qrCodeContent = HttpContext.Session.GetString("CurrentQRCode");
            if (string.IsNullOrEmpty(qrCodeContent))
            {
                return NotFound("QR Code non trovato");
            }

            var qrCode = await _qrCodeService.GetQRCodeByContentAsync(qrCodeContent);
            if (qrCode == null)
            {
                return View("QRCodeInvalid", new { QRCodeContent = qrCodeContent });
            }

            var viewModel = new QRCodeAccessViewModel
            {
                QRCodeContent = qrCodeContent,
                RequiresPassword = qrCode.UsePassword,
                AllowsDownload = qrCode.AllowDownload,
                DestinationLink = qrCode.DestinationLink,
                Details = qrCode.Details
            };

            if (!qrCode.UsePassword && !qrCode.AllowDownload && !string.IsNullOrEmpty(qrCode.DestinationLink))
            {
                return Redirect(qrCode.DestinationLink);
            }

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAccess(QRCodeAccessViewModel model)
        {
            var qrCode = await _qrCodeService.GetQRCodeByContentAsync(model.QRCodeContent);
            if (qrCode == null)
            {
                return NotFound();
            }

            if (qrCode.UsePassword && qrCode.Password != model.Password)
            {
                TempData["ErrorMessage"] = "Password errata!";
                model.RequiresPassword = qrCode.UsePassword;
                model.AllowsDownload = qrCode.AllowDownload;
                model.DestinationLink = qrCode.DestinationLink;
                model.Details = qrCode.Details;
                return View("Access", model);
            }

            if (!string.IsNullOrEmpty(qrCode.DestinationLink))
            {
                return Redirect(qrCode.DestinationLink);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FreeAccess(QRCodeAccessViewModel model)
        {
            var qrCode = await _qrCodeService.GetQRCodeByContentAsync(model.QRCodeContent);
            if (qrCode == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(qrCode.DestinationLink))
            {
                return Redirect(qrCode.DestinationLink);
            }

            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Download(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            var qrCode = await _qrCodeService.GetQRCodeByContentAsync(code);
            if (qrCode == null || !qrCode.AllowDownload)
            {
                return Forbid();
            }

            var pdfBytes = await _qrCodeService.GenerateQRCodePdfAsync(qrCode.Content, qrCode.Details ?? string.Empty, qrCode.CodiceUtilizzo ?? string.Empty);
            return File(pdfBytes, "application/pdf", $"QRCode_{code}.pdf");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Image(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }

            var qrCode = await _qrCodeService.GetQRCodeByContentAsync(code);
            if (qrCode == null)
            {
                return NotFound();
            }

            var imageBytes = await _qrCodeService.GenerateQRCodeImageAsync(qrCode.Content);
            return File(imageBytes, "image/png");
        }

        [AllowAnonymous]
        public IActionResult QRCodeInvalid(string qrCodeContent = "")
        {
            ViewBag.QRCodeContent = qrCodeContent;
            return View();
        }

        [AllowAnonymous]
        public IActionResult CodesiaNotFound(string code = "", string qrCodeContent = "")
        {
            // Se viene passato il parametro "code" dalla route, costruisci l'URL completo
            if (!string.IsNullOrEmpty(code))
            {
                qrCodeContent = $"{Request.Scheme}://{Request.Host}/s={code}";
            }

            ViewBag.QRCodeContent = qrCodeContent;
            return View();
        }

        [AllowAnonymous]
        public IActionResult CodeDisabled()
        {
            return View();
        }

        private async Task<bool> IsCodeAuthorized(string code, ApplicationUser? user)
        {
            if (user?.CompanyId == null) return false;

            var ranges = await _context.CodeRanges
                .Where(cr => cr.CompanyId == user.CompanyId)
                .ToListAsync();

            foreach (var range in ranges)
            {
                if (IsCodeInRange(code, range.CodiceIniziale, range.CodiceFinale))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsCodeInRange(string code, string startCode, string endCode)
        {
            // Implementazione semplificata - confronto alfanumerico
            return string.Compare(code, startCode, StringComparison.OrdinalIgnoreCase) >= 0 &&
                   string.Compare(code, endCode, StringComparison.OrdinalIgnoreCase) <= 0;
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
                EntitaId = entitaId is string ? null : (int?)entitaId,
                Dettagli = dettagli,
                IPAddress = ipAddress
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
