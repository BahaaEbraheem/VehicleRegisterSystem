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
            modelBuilder.Entity<Order>(entity =>
            {
                // Clustered على Id (مفتاح أساسي تلقائي)
                entity.HasKey(o => o.Id);

                // Non-clustered indexes
                entity.HasIndex(o => o.EngineNumber).IsUnique(); // فحص التكرار
                entity.HasIndex(o => o.BoardNumber).IsUnique(false); // بحث حسب اللوحة
                entity.HasIndex(o => o.NationalNumber); // البحث حسب الرقم الوطني
                entity.HasIndex(o => o.Status); // البحث حسب حالة الطلب
                entity.HasIndex(o => o.CreatedById); // جلب الطلبات الخاصة بالمستخدم
                entity.HasIndex(o => o.CreatedAt); // إذا كنت ترتب الطلبات حسب التاريخ
            });
        }
    }
}
