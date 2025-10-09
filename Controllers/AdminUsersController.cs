using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;
using QRCodeManagerRelease2.Services;
using System.Security.Claims;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize(Roles = "Admin,Direzione")]
    [Route("Admin/[action]")]
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExportService _exportService;

        public AdminUsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IExportService exportService)
        {
            _context = context;
            _userManager = userManager;
            _exportService = exportService;
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.CustomerGroup)
                .Include(u => u.Company)
                .ToListAsync();
            
            ViewBag.CustomerGroups = await _context.CustomerGroups.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            
            return View(users);
        }

        public async Task<IActionResult> CreateUser()
        {
            ViewBag.CustomerGroups = await _context.CustomerGroups.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsAzienda = model.IsAzienda,
                    NomeAzienda = model.NomeAzienda,
                    Via = model.Via,
                    PartitaIva = model.PartitaIva,
                    Telefono = model.Telefono,
                    Note = model.Note,
                    CustomerGroupId = model.CustomerGroupId,
                    CompanyId = model.CompanyId,
                    Abilitato = model.Abilitato,
                    DataRegistrazione = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    await LogActivity("Creazione", "Utente", user.Id, $"Nuovo utente creato: {user.Email}");

                    if(user.CustomerGroupId == "group-direzione")
                    {
                        await _userManager.AddToRoleAsync(user, "Direzione");
                    }
                    
                    TempData["SuccessMessage"] = "Utente creato con successo!";
                    return RedirectToAction("Users");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.CustomerGroups = await _context.CustomerGroups.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();

            return RedirectToAction("PendingUsers");
            // return View(model);
        }

        public async Task<IActionResult> PendingUsers()
        {
            var pendingUsers = await _context.Users
                .Where(u => !u.Abilitato)
                .ToListAsync();
                
            ViewBag.CustomerGroups = await _context.CustomerGroups.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            
            return View(pendingUsers);
        }

        [HttpPost]
        public async Task<IActionResult> EnableUser(string userId, string customerGroupId, int? companyId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Abilitato = true;
                user.CustomerGroupId = customerGroupId;
                user.CompanyId = companyId;

                await _userManager.UpdateAsync(user);
                
                await LogActivity("Abilitazione", "Utente", userId, $"Utente {user.Email} abilitato");
                
                TempData["SuccessMessage"] = "Utente abilitato con successo!";
            }
            return RedirectToAction("PendingUsers");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(string userId, string firstName, string lastName, string email, string customerGroupId, int? companyId, bool abilitato, string? nomeAzienda, string? via, string? partitaIva, string? telefono, string? note)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if(user.CustomerGroupId == "group-direzione" && customerGroupId != "group-direzione")
            {
                await _userManager.RemoveFromRoleAsync(user, "Direzione");
            }

            if(user.CustomerGroupId != "group-direzione" && customerGroupId == "group-direzione")
            {
                await _userManager.AddToRoleAsync(user, "Direzione");
            }

            if (user != null)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                user.Email = email;
                user.UserName = email;
                user.Abilitato = abilitato;
                user.CustomerGroupId = customerGroupId;
                user.CompanyId = companyId;
                user.NomeAzienda = nomeAzienda;
                user.Via = via;
                user.PartitaIva = partitaIva;
                user.Telefono = telefono;
                user.Note = note;

                await _userManager.UpdateAsync(user);
                
                await LogActivity("Modifica", "Utente", userId, $"Utente {user.Email} modificato");
                
                TempData["SuccessMessage"] = "Utente modificato con successo!";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Abilitato = !user.Abilitato;
                await _userManager.UpdateAsync(user);
                
                string action = user.Abilitato ? "Abilitazione" : "Disabilitazione";
                await LogActivity(action, "Utente", userId, $"Utente {user.Email} {(user.Abilitato ? "abilitato" : "disabilitato")}");
                
                TempData["SuccessMessage"] = $"Utente {(user.Abilitato ? "abilitato" : "disabilitato")} con successo!";
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utente non trovato!";
                return RedirectToAction("Users");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                await LogActivity("Reset Password", "Utente", userId, $"Password reimpostata per {user.Email}");
                TempData["SuccessMessage"] = "Password reimpostata con successo!";
            }
            else
            {
                TempData["ErrorMessage"] = "Errore nel reset della password: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Users");
        }


        [HttpGet]
        public async Task<IActionResult> ExportUsersExcel()
        {
            var users = await _context.Users
                .Include(u => u.CustomerGroup)
                .Include(u => u.Company)
                .ToListAsync();

            var fileContents = await _exportService.ExportUsersToExcel(users);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Utenti_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportUsersPdf()
        {
            var users = await _context.Users
                .Include(u => u.CustomerGroup)
                .Include(u => u.Company)
                .ToListAsync();

            var fileContents = await _exportService.ExportUsersToPdf(users);
            return File(fileContents, "application/pdf", $"Utenti_{DateTime.Now:yyyyMMdd}.pdf");
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
