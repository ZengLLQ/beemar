using BeeMart.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Data;

public class BeeMartDbContext : DbContext
{
    public BeeMartDbContext(DbContextOptions<BeeMartDbContext> options) : base(options)
    {
    }

    public DbSet<MsUser> Users => Set<MsUser>();
    public DbSet<LtPaymentMethod> PaymentMethods => Set<LtPaymentMethod>();
    public DbSet<MsProduct> Products => Set<MsProduct>();
    public DbSet<TrTransaction> Transactions => Set<TrTransaction>();
    public DbSet<TrTransactionDetail> TransactionDetails => Set<TrTransactionDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MsUser
        modelBuilder.Entity<MsUser>(entity =>
        {
            entity.HasKey(e => e.IdUser);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // LtPaymentMethod
        modelBuilder.Entity<LtPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.IdPaymentMethod);
        });

        // MsProduct
        modelBuilder.Entity<MsProduct>(entity =>
        {
            entity.HasKey(e => e.IdProduct);
            entity.Property(e => e.Weight).HasPrecision(10, 2);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(5, 2);
        });

        // TrTransaction
        modelBuilder.Entity<TrTransaction>(entity =>
        {
            entity.HasKey(e => e.IdTransaction);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(e => e.IdUser)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PaymentMethod)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(e => e.IdPaymentMethod)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // TrTransactionDetail
        modelBuilder.Entity<TrTransactionDetail>(entity =>
        {
            entity.HasKey(e => e.IdTransactionDetail);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(5, 2);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Transaction)
                  .WithMany(t => t.TransactionDetails)
                  .HasForeignKey(e => e.IdTransaction)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.TransactionDetails)
                  .HasForeignKey(e => e.IdProduct)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
