using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Prog2bPOEPart2.Models;

namespace Prog2bPOEPart2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Prog2bPOEPart2.Models.Claim> Claim { get; set; } = default!;
        public DbSet<Prog2bPOEPart2.Models.Document> Document { get; set; } = default!;
    }
}
