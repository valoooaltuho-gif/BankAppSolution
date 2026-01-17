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

            var rowsAffected = await _context.Accounts
        .Where(a => a.Id == account.Id)
        .ExecuteUpdateAsync(updates => updates
            .SetProperty(a => a.Balance, account.Balance)
            .SetProperty(a => a.OwnerName, account.OwnerName)
        // Добавьте сюда все остальные свойства, которые могут меняться
        );

            if (rowsAffected == 0)
            {
                throw new DbUpdateConcurrencyException("The database operation was expected to affect 1 row(s), but actually affected 0 row(s); data may have been modified or deleted since entities were loaded.");
            }

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
