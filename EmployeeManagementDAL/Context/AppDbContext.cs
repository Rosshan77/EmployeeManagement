using EmployeeManagementDAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementDAL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Department)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(x => x.Salary)
                      .HasPrecision(18, 2);

                entity.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Username)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(x => x.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(x => x.Password)
                      .IsRequired();

                entity.Property(x => x.Role)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();
            });
        }
    }
}
