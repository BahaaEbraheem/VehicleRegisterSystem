using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRegisterSystem.Domain;
using VehicleRegisterSystem.Domain.Entities;

namespace VehicleRegisterSystem.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.EngineNumber);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.BoardNumber)
                .IsUnique(false); // لاحقاً يمكن تغييره إلى true مع منطق التحقق
        }
    }
}
