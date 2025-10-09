
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using System.Security.Claims;

namespace QRCodeManagerRelease2.Controllers
{
    [Authorize(Roles = "Admin, Direzione")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var pendingUsers = await _context.Users
                .Where(u => !u.Abilitato)
                .ToListAsync();

            var anomalies = await _context.Anomalies
                .Where(a => !a.Risolta)
                .Include(a => a.User)
                .ToListAsync();

            ViewBag.PendingUsersCount = pendingUsers.Count;
            ViewBag.AnomaliesCount = anomalies.Count;

            return View();
        }

        protected async Task LogActivity(string azione, string entita, object entitaId, string dettagli)
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
