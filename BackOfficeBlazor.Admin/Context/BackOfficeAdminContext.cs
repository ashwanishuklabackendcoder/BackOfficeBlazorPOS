using BackOfficeBlazor.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeBlazor.Admin.Context
{
    public class BackOfficeAdminContext : DbContext
    {
        public BackOfficeAdminContext(DbContextOptions<BackOfficeAdminContext> options)
            : base(options)
        {
        }

        public DbSet<Category> _Categories => Set<Category>();
        public DbSet<Manufacturer> _Makes { get; set; }
        public DbSet<Supplier> _Suppliers { get; set; }
        public DbSet<FTT05> FTT05 { get; set; }
        public DbSet<FTT11> FTT11 { get; set; }

        public DbSet<Location> _Locations { get; set; }
        public DbSet<Setting> Settings { get; set; }

        public DbSet<Stock> _ProductsStock { get; set; }
        public DbSet<StockLevels> _ProductsStockLevels { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ProductItem> ProductItems { get; set; }
        public DbSet<ProductLevel> ProductLevels { get; set; }

        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<ProductGroupItem> ProductGroupItems { get; set; }
        public DbSet<ProductStockMovement> ProductStockMovement { get; set; }
        public DbSet<Layaway> Layaways { get; set; }
        public DbSet<StaffUser> StaffUsers { get; set; }
        public DbSet<StaffUserPermission> StaffUserPermissions { get; set; }
        public DbSet<StaffUserPermissionEntry> StaffUserPermissionEntries { get; set; }
        public DbSet<PrinterConfig> PrinterConfigs { get; set; }
        public DbSet<TerminalOption> TerminalOptions { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<SysOption> SysOptions { get; set; }
        public DbSet<QuickShortcutItem> QuickShortcutItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StaffUser>()
                .HasIndex(x => x.Username)
                .IsUnique();

            modelBuilder.Entity<StaffUser>()
                .HasOne(x => x.Permission)
                .WithOne(x => x.User)
                .HasForeignKey<StaffUserPermission>(x => x.StaffUserId);

            modelBuilder.Entity<StaffUserPermissionEntry>()
                .HasIndex(x => new { x.StaffUserId, x.PermissionKey })
                .IsUnique();

            modelBuilder.Entity<StaffUserPermissionEntry>()
                .HasOne(x => x.User)
                .WithMany(x => x.PermissionEntries)
                .HasForeignKey(x => x.StaffUserId);

            modelBuilder.Entity<Location>()
                .HasIndex(x => x.Code)
                .IsUnique();

            modelBuilder.Entity<Setting>()
                .HasKey(x => x.BranchId);

            modelBuilder.Entity<PrinterConfig>()
                .HasIndex(x => new { x.LocationCode, x.PrinterName });

            modelBuilder.Entity<TerminalOption>()
                .HasIndex(x => x.TerminalCode)
                .IsUnique();

            modelBuilder.Entity<TerminalOption>()
                .HasOne<PrinterConfig>()
                .WithMany()
                .HasForeignKey(x => x.ReceiptPrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TerminalOption>()
                .HasOne<PrinterConfig>()
                .WithMany()
                .HasForeignKey(x => x.A4PrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TerminalOption>()
                .HasOne<PrinterConfig>()
                .WithMany()
                .HasForeignKey(x => x.LabelPrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuickShortcutItem>()
                .HasIndex(x => x.DisplayOrder);
        }

    }
}
