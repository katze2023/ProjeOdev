using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace FitnessCenterManagement.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdmin(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. ROLLERİ OLUŞTUR
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));



            // 2. VARSAYILAN ADMIN KULLANICISI
            string adminEmail = "admin@fit.com";
            string adminPassword = "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,

                    // Varsayılan profil bilgileri (zorunlu kolonlardan kaçınmak için)
                    HeightCm = 180,
                    WeightKg = 75,
                    BodyType = "Athletic",
                    ProfileImagePath = null
                };

                // Kullanıcıyı oluştur
                var createResult = await userManager.CreateAsync(admin, adminPassword);

                // Eğer oluşturulmadıysa bağlantılı tüm hataları göster
                if (!createResult.Succeeded)
                {
                    string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("Admin kullanıcı oluşturulamadı: " + errors);
                }

                // Rol ata
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
