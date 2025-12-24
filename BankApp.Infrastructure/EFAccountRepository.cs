using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BankApp.Core.Models;

namespace BankApp.Infrastructure
{
    public class EFAccountRepository : IAccountRepository
    {
        private readonly BankContext _context;

        public EFAccountRepository(BankContext context)
        {
            _context = context;
        }

        public async Task<Account> GetByIdAsync(Guid id)
        {
            // Используем Include для загрузки связанных транзакций
            return await _context.Accounts
                                 .Include(a => a.Transactions)
                                 .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync(); // Сохраняем в БД
        }

        public async Task UpdateAsync(Account account)
        {
            // EF Core отслеживает изменения автоматически, достаточно SaveChangesAsync
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            // Используем Include для загрузки связанных транзакций
            return await _context.Accounts
                                 .Include(a => a.Transactions)
                                 .ToListAsync();
        }
    }
}
