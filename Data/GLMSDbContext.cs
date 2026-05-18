using Logistics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace Logistics.Data
{
    public class GLMSDbContext : DbContext
    {
        public GLMSDbContext(DbContextOptions<GLMSDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Contract> Contracts { get; set; }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        public DbSet<ServiceLevel> ServiceLevels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Contracts)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId);

            modelBuilder.Entity<Contract>()
                .HasMany(c => c.ServiceRequests)
                .WithOne(sr => sr.Contract)
                .HasForeignKey(sr => sr.ContractId);
        }
    }
}
