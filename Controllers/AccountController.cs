    using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;
using QRCodeManagerRelease2.Services;
using System.Security.Claims;
using ZXing.QrCode.Internal;

namespace QRCodeManagerRelease2.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsAzienda = true,// model.IsAzienda,
                    NomeAzienda = model.NomeAzienda,
                    Via = model.Via,
                    PartitaIva = model.PartitaIva,
                    Telefono = model.Telefono,
                    Note = model.Note,
                    DataRegistrazione = DateTime.Now,
                    Abilitato = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    await LogActivity("Registrazione", "Utente", user.Id, $"Nuovo utente registrato: {user.Email}");
                    
                    // Invia email di notifica della registrazione
                    await _emailService.SendRegistrationNotificationAsync(
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.IsAzienda,
                        user.NomeAzienda,
                        user.Via,
                        user.PartitaIva,
                        user.Telefono,
                        user.Note
                    );
                    
                    if (model.IsAzienda)
                    {
                        ViewBag.IsAzienda = true;
                    }

                   

                    return View("RegisterSuccess");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    await LogActivity("Login", "Utente", user.Id, $"Utente {email} ha effettuato il login");

                    // 👉 1. PRIMA controlla se c'è un'azione specifica da completare.
                    // Questo vale per TUTTI gli utenti, admin inclusi.
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // 👉 2. POI, se non c'è un returnUrl, applica la logica di default basata sul ruolo.
                    // Se è admin, reindirizza alla dashboard admin.
                    if (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "Direzione"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }

                    // 👉 3. Infine, il reindirizzamento di default per tutti gli altri utenti.
                    return RedirectToAction("Dashboard", "Home");
                }

                ModelState.AddModelError(string.Empty, "Login non valido.");
            }

            return View(); // Se il modello non è valido, ripresenta la vista del login
        }

        //[HttpPost]
        //public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);

        //        if (result.Succeeded)
        //        {
        //            var user = await _userManager.FindByEmailAsync(email);
        //            await LogActivity("Login", "Utente", user.Id, $"Utente {email} ha effettuato il login");

        //            // Se è admin, reindirizza alla dashboard admin
        //            if (await _userManager.IsInRoleAsync(user, "Admin"))
        //            {
        //                return RedirectToAction("Dashboard", "Admin");
        //            }

        //            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        //            {
        //                return Redirect(returnUrl);
        //            }

        //            return RedirectToAction("Dashboard", "Home");
        //        }

        //        ModelState.AddModelError(string.Empty, "Login non valido.");
        //    }

        //    return View();
        //}

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await LogActivity("Logout", "Utente", userId, "Logout effettuato");
            
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult PrivateRegistration()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string email, 
            string telefono, string nomeAzienda, string via, string partitaIva, string note,
            string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Aggiorna i dati del profilo
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.UserName = email;
            user.Telefono = telefono;
            
            if (user.IsAzienda)
            {
                user.NomeAzienda = nomeAzienda;
                user.Via = via;
                user.PartitaIva = partitaIva;
                user.Note = note;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            // Gestione cambio password se richiesto
            if (!string.IsNullOrEmpty(currentPassword) && !string.IsNullOrEmpty(newPassword))
            {
                if (newPassword == confirmPassword)
                {
                    var passwordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Le password non corrispondono");
                    return View();
                }
            }

            if (updateResult.Succeeded)
            {
                await LogActivity("Modifica", "Profilo", user.Id, "Profilo utente aggiornato");
                TempData["SuccessMessage"] = "Profilo aggiornato con successo!";
                //return RedirectToAction("Profile");
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
           // return RedirectToAction("Index", "Home");
            return View();
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
