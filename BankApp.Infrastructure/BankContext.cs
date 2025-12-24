using Microsoft.EntityFrameworkCore;
using BankApp.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 

namespace BankApp.Infrastructure
{
    public class BankContext : IdentityDbContext
    {
        public BankContext(DbContextOptions<BankContext> options) : base(options)
        {
        }

        // DbSets представляют таблицы в базе данных
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ApiUser> ApiUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Указываем EF Core, что Account является базовым типом для наследования
            modelBuilder.Entity<Account>()
                .HasDiscriminator<string>("account_type")
                .HasValue<CheckingAccount>("checking")
                .HasValue<SavingsAccount>("savings")
                .HasValue<BusinessAccount>("business");

            // Настройка связи: У Аккаунта много Транзакций
            modelBuilder.Entity<Account>()
                .HasMany(a => a.Transactions)
                .WithOne()
                .IsRequired();
            
           
            base.OnModelCreating(modelBuilder);
        }
    }
}
