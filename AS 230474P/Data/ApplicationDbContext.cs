using Microsoft.EntityFrameworkCore;
using AS_230474P.Models; // Ensure this is included for accessing your RegistrationModel

namespace AS_230474P.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Explicitly define the table name and schema
        public DbSet<RegistrationModel> Registrations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicitly specify schema if it's not the default dbo
            modelBuilder.Entity<RegistrationModel>().ToTable("Registrations", "dbo");
        }
    }

}
