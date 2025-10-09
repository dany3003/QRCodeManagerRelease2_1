
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

            if (!await roleManager.RoleExistsAsync("Direzione"))
            {
                await roleManager.CreateAsync(new IdentityRole("Direzione"));
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

                var direzioneGroup = new CustomerGroup
                {
                    Id = "group-direzione",
                    Name = "Direzione",
                    Description = "Gruppo della direzione"
                };

                context.CustomerGroups.AddRange(adminGroup, clientGroup, direzioneGroup);
                await context.SaveChangesAsync();
            }

            //IMPLEMENTAZIONE GROUP-DIREZIONE, SE NON ESISTE, CREA IL GRUPPO
            var direzioneGroupExist = await context.CustomerGroups.FindAsync("group-direzione");
            if (direzioneGroupExist == null)
            {
                var direzioneGroup = new CustomerGroup
                {
                    Id = "group-direzione",
                    Name = "Direzione",
                    Description = "Gruppo della direzione"
                };
                context.CustomerGroups.AddRange(direzioneGroup);
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
                    UserName = "user@qrmanager.com",
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

            if (await userManager.FindByNameAsync("direzione@qrmanager.com") == null)
            {
                var clientUser = new ApplicationUser
                {
                    UserName = "direzione@qrmanager.com",
                    Email = "direzione@qrmanager.com",
                    EmailConfirmed = true,
                    FirstName = "Direzione",
                    LastName = "User",
                    CustomerGroupId = "group-direzione"
                };

                var result = await userManager.CreateAsync(clientUser, "direzione");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(clientUser, "Direzione");
                }
            }
        }
    }
}
