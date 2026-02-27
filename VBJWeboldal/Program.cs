using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VBJWeboldal.Data;
using VBJWeboldal.Models;

namespace VBJWeboldal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Adatbázis kapcsolat konfigurálása
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // 2. ASP.NET Identity konfigurálása
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //Autentikáció bekapcsolása
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // SZEREPKÖRÖK LÉTREHOZÁSA INDULÁSKOR
            // SZEREPKÖRÖK LÉTREHOZÁSA ÉS AZ ELSÕ ADMIN KINEVEZÉSE
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Behozzuk a UserManager-t is a fiókok kezeléséhez
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Admin", "Editor", "Reader", "GalleryManager" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // --- AZ ELSÕ ADMIN KINEVEZÉSE ---
                // IDE ÍRD BE AZT AZ EMAIL CÍMET, AMIVEL REGISZTRÁLTÁL:
                var myAdminEmail = "admin@test.com";

                var adminUser = await userManager.FindByEmailAsync(myAdminEmail);
                if (adminUser != null)
                {
                    // Ha létezik a fiók, és még nem Admin, akkor azzá tesszük!
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }

            app.Run();
        }
    }
}