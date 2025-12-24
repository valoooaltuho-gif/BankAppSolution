using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Core.Models
{
    public interface IAccountRepository
    {
        // Методы для работы с аккаунтами
        Task<Account> GetByIdAsync(Guid id);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task<IEnumerable<Account>> GetAllAsync();
    }
}
