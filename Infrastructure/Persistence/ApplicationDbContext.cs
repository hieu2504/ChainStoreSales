using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChainStoreSalesManagement.Domain.Entities;
using ChainStoreSalesManagement.Domain.Entities.Views;

namespace ChainStoreSalesManagement.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet entities
        public DbSet<Shop> Shops { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<Inventory> Inventories { get; set; } = null!;
        public DbSet<InventorySerial> InventorySerials { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<CouponProduct> CouponProducts { get; set; } = null!;
        public DbSet<CouponRedemption> CouponRedemptions { get; set; } = null!;
        public DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderLine> OrderLines { get; set; } = null!;
        public DbSet<OrderCoupon> OrderCoupons { get; set; } = null!;
        public DbSet<OrderLineSerial> OrderLineSerials { get; set; } = null!;
        public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public DbSet<ShopSubscription> ShopSubscriptions { get; set; } = null!;
        public DbSet<BillingTransaction> BillingTransactions { get; set; } = null!;
        public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;
        public DbSet<Feature> Features { get; set; } = null!;
        public DbSet<RoleFeature> RoleFeatures { get; set; } = null!;

        // View entities (Keyless)
        public DbSet<RevenueDaily> RevenueDailies { get; set; } = null!;
        public DbSet<PaymentHistory> PaymentHistories { get; set; } = null!;
        public DbSet<PersonalSales> PersonalSales { get; set; } = null!;
        public DbSet<vw_UserEffectiveFeatures> UserEffectiveFeatures { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default delete behavior to NoAction to avoid cascade conflicts
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // =============== 1. UNIQUE & COMPOSITE UNIQUE CONSTRAINTS ===============
            
            // Shop
            modelBuilder.Entity<Shop>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Branch
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasIndex(e => new { e.ShopId, e.Code }).IsUnique();
                entity.Property(e => e.RowVersion).IsRowVersion();
            });

            // ProductCategory
            modelBuilder.Entity<ProductCategory>(entity =>
            {
                // Removed unique index on ShopId to allow null values
            });

            // Brand
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasIndex(e => new { e.ShopId, e.Name }).IsUnique();
            });

            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => new { e.ShopId, e.Code })
                    .IsUnique()
                    .HasFilter("[Code] IS NOT NULL");
                entity.Property(e => e.RowVersion).IsRowVersion();
            });

            // ProductVariant
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasIndex(e => new { e.ProductId, e.Sku }).IsUnique();
                entity.HasIndex(e => e.Sku)
                    .IncludeProperties(e => new { e.ProductId, e.TrackSerial });
                entity.Property(e => e.RowVersion).IsRowVersion();
            });

            // InventorySerial
            modelBuilder.Entity<InventorySerial>(entity =>
            {
                entity.HasIndex(e => new { e.ShopId, e.BranchId, e.VariantId, e.SerialNo }).IsUnique();
                
                entity.HasOne(e => e.Shop)
                    .WithMany(s => s.InventorySerials)
                    .HasForeignKey(e => e.ShopId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(e => e.Branch)
                    .WithMany(b => b.InventorySerials)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(e => e.ProductVariant)
                    .WithMany(v => v.InventorySerials)
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => new { e.ShopId, e.UserId }).IsUnique();
                entity.HasOne(e => e.Shop)
                    .WithMany(s => s.Employees)
                    .HasForeignKey(e => e.ShopId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Employees)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Branch)
                    .WithMany(b => b.Employees)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => new { e.ShopId, e.Code })
                    .IsUnique()
                    .HasFilter("[Code] IS NOT NULL");
                entity.HasIndex(e => new { e.ShopId, e.Phone, e.Email });
                entity.Property(e => e.RowVersion).IsRowVersion();
            });

            // RoleFeature
            modelBuilder.Entity<RoleFeature>(entity =>
            {
                entity.HasIndex(e => new { e.RoleId, e.FeatureId }).IsUnique();
            });

            // =============== 2. KEYS & FOREIGN KEYS ===============

            // PaymentMethod
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.MethodCode);
            });

            // Inventory (Composite Key)
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => new { e.ShopId, e.BranchId, e.VariantId });
                entity.HasIndex(e => new { e.ShopId, e.BranchId, e.VariantId });
                
                entity.HasOne(e => e.Shop)
                    .WithMany(s => s.Inventories)
                    .HasForeignKey(e => e.ShopId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(e => e.Branch)
                    .WithMany(b => b.Inventories)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(e => e.ProductVariant)
                    .WithMany(v => v.Inventories)
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // CouponProduct (Composite Key)
            modelBuilder.Entity<CouponProduct>(entity =>
            {
                entity.HasKey(e => new { e.CouponId, e.VariantId });
                entity.HasOne(e => e.Coupon)
                    .WithMany(c => c.CouponProducts)
                    .HasForeignKey(e => e.CouponId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ProductVariant)
                    .WithMany(v => v.CouponProducts)
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // OrderCoupon (Composite Key)
            modelBuilder.Entity<OrderCoupon>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.CouponId });
            });

            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.OrderNo).IsUnique();
                entity.HasIndex(e => e.OrderDate)
                    .IsDescending()
                    .IncludeProperties(e => new { e.Status, e.TotalAmount, e.SalesUserId, e.ShopId, e.BranchId });
                
                entity.Property(e => e.TotalAmount)
                    .HasComputedColumnSql("(SubTotal - Discount + ShippingFee + Tax)", stored: true);
                entity.Property(e => e.RowVersion).IsRowVersion();

                entity.HasOne(e => e.OrderStatus)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(e => e.Status)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.SalesUser)
                    .WithMany(u => u.SalesOrders)
                    .HasForeignKey(e => e.SalesUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Shop)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(e => e.ShopId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(e => e.Branch)
                    .WithMany(b => b.Orders)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.NoAction);
                    
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // OrderLine
            modelBuilder.Entity<OrderLine>(entity =>
            {
                entity.HasIndex(e => new { e.OrderId, e.VariantId }).IsUnique();
                
                entity.Property(e => e.Amount)
                    .HasComputedColumnSql("((UnitPrice * Qty) - LineDiscount)", stored: true);
                entity.Property(e => e.RowVersion).IsRowVersion();

                entity.ToTable(t => t.HasCheckConstraint("CK_OrderLine_Qty_Positive", "[Qty] > 0"));

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderLines)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderLineSerial
            modelBuilder.Entity<OrderLineSerial>(entity =>
            {
                entity.HasIndex(e => new { e.OrderLineId, e.SerialNo }).IsUnique();
                
                entity.HasOne(e => e.OrderLine)
                    .WithMany(ol => ol.OrderLineSerials)
                    .HasForeignKey(e => e.OrderLineId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.Payments)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ErrorLog
            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasIndex(e => e.LogTime).IsDescending();
            });

            // BillingTransaction
            modelBuilder.Entity<BillingTransaction>(entity =>
            {
                entity.HasKey(e => e.BillId);
            });

            // CouponRedemption
            modelBuilder.Entity<CouponRedemption>(entity =>
            {
                entity.HasKey(e => e.RedemptionId);
            });

            // ErrorLog
            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
            });

            // CouponProduct (junction table)
            modelBuilder.Entity<CouponProduct>(entity =>
            {
                entity.HasKey(e => new { e.CouponId, e.VariantId });
            });

            // OrderCoupon (junction table)
            modelBuilder.Entity<OrderCoupon>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.CouponId });
            });

            // RoleFeature
            modelBuilder.Entity<RoleFeature>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // OrderLineSerial
            modelBuilder.Entity<OrderLineSerial>(entity =>
            {
                entity.HasKey(e => e.OrderLineSerialId);
            });

            // InventorySerial
            modelBuilder.Entity<InventorySerial>(entity =>
            {
                entity.HasKey(e => e.SerialId);
            });

            // Basic entities primary keys
            modelBuilder.Entity<Shop>(entity => entity.HasKey(e => e.ShopId));
            modelBuilder.Entity<Branch>(entity => entity.HasKey(e => e.BranchId));
            modelBuilder.Entity<ProductCategory>(entity => entity.HasKey(e => e.CategoryId));
            modelBuilder.Entity<Brand>(entity => entity.HasKey(e => e.BrandId));
            modelBuilder.Entity<Product>(entity => entity.HasKey(e => e.ProductId));
            modelBuilder.Entity<ProductVariant>(entity => entity.HasKey(e => e.VariantId));
            modelBuilder.Entity<Inventory>(entity => entity.HasKey(e => new { e.ShopId, e.BranchId, e.VariantId }));
            modelBuilder.Entity<Employee>(entity => entity.HasKey(e => e.EmployeeId));
            modelBuilder.Entity<Customer>(entity => entity.HasKey(e => e.CustomerId));
            modelBuilder.Entity<Coupon>(entity => entity.HasKey(e => e.CouponId));
            modelBuilder.Entity<OrderStatus>(entity => entity.HasKey(e => e.StatusCode));
            modelBuilder.Entity<Order>(entity => entity.HasKey(e => e.OrderId));
            modelBuilder.Entity<OrderLine>(entity => entity.HasKey(e => e.OrderLineId));
            modelBuilder.Entity<PaymentMethod>(entity => entity.HasKey(e => e.MethodCode));
            modelBuilder.Entity<Payment>(entity => entity.HasKey(e => e.PaymentId));
            modelBuilder.Entity<SubscriptionPlan>(entity => entity.HasKey(e => e.PlanId));
            modelBuilder.Entity<ShopSubscription>(entity => entity.HasKey(e => e.ShopSubId));

            // Feature
            modelBuilder.Entity<Feature>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // SubscriptionPlan
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // ShopSubscription
            modelBuilder.Entity<ShopSubscription>(entity =>
            {
                entity.HasIndex(e => new { e.ShopId, e.PlanId, e.StartDate }).IsUnique();
            });

            // ProductImage configuration (existing)
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImage");
                
                entity.HasKey(e => e.ImageId);
                
                entity.Property(e => e.FilePath)
                    .HasMaxLength(400)
                    .IsRequired();
                
                entity.Property(e => e.AltText)
                    .HasMaxLength(200);
                
                entity.Property(e => e.RowVersion)
                    .IsRowVersion();
                
                entity.Property(e => e.SortOrder)
                    .HasDefaultValue(0);
                
                entity.Property(e => e.IsPrimary)
                    .HasDefaultValue(false);
                
                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("SYSUTCDATETIME()");
                
                entity.HasIndex(e => new { e.ProductId, e.SortOrder });
                
                entity.HasIndex(e => new { e.ProductId, e.IsPrimary })
                    .IncludeProperties(e => e.FilePath);
                
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =============== 7. KEYLESS ENTITIES FOR VIEWS ===============

            modelBuilder.Entity<RevenueDaily>()
                .HasNoKey()
                .ToView("RevenueDaily", "rpt");

            modelBuilder.Entity<PaymentHistory>()
                .HasNoKey()
                .ToView("PaymentHistory", "rpt");

            modelBuilder.Entity<PersonalSales>()
                .HasNoKey()
                .ToView("PersonalSales", "rpt");

            modelBuilder.Entity<vw_UserEffectiveFeatures>()
                .HasNoKey()
                .ToView("vw_UserEffectiveFeatures");
        }
    }
}
