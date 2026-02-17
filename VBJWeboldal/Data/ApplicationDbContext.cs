using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VBJWeboldal.Models;

namespace VBJWeboldal.Data
{
    //átadjuk az ApplicationUser-t
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Itt kötjük be a már meglévő modelleket táblaként az adatbázisba:
        public DbSet<News> News { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<Image> Images { get; set; }

        // Később ide jöhet az Article és Message is!
    }
}