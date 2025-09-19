
using Microsoft.AspNetCore.Identity;
using QRCodeManagerRelease2.Models;

namespace QRCodeManagerRelease2.Data
{
    public static class SeedData
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            
            if (!context.CustomerGroups.Any())
            {
                var adminGroup = new CustomerGroup
                {
                    Id = "group-admin",
                    Name = "Amministratori",
                    Description = "Gruppo degli amministratori del sistema"
                };
                
                var clientGroup = new CustomerGroup
                {
                    Id = "group-clients", 
                    Name = "Clienti",
                    Description = "Gruppo dei clienti standard"
                };
                
                context.CustomerGroups.AddRange(adminGroup, clientGroup);
                await context.SaveChangesAsync();
            }
            
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@qrmanager.com",
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    CustomerGroupId = "group-admin"
                };
                
                var result = await userManager.CreateAsync(adminUser, "admin");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            
            if (await userManager.FindByNameAsync("user") == null)
            {
                var clientUser = new ApplicationUser
                {
                    UserName = "user",
                    Email = "user@qrmanager.com",
                    EmailConfirmed = true,
                    FirstName = "Cliente",
                    LastName = "User",
                    CustomerGroupId = "group-clients"
                };
                
                var result = await userManager.CreateAsync(clientUser, "user");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(clientUser, "User");
                }
            }
        }
    }
}
