using Microsoft.EntityFrameworkCore;
using PontelloImport.Models;

namespace PontelloImport.Data
{
    public class PontelloDbContext : DbContext
    {
        public PontelloDbContext(DbContextOptions<PontelloDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Product { get; set; }
    }
  
}
