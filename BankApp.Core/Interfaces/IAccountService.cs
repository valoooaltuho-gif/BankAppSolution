using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Core.Models
{
    public interface IAccountService
    {
        Task<Account> CreateAccountAsync(string accountType, string ownerName, decimal initialDeposit);
        Task DepositAsync(Guid accountId, decimal amount);
        Task RunMonthlyProcessAsync();
        Task WithdrawAsync(Guid accountId, decimal amount);
        Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount);
        Task<IEnumerable<Transaction>> GetStatementAsync(Guid accountId);
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account> GetByIdAsync(Guid id);
    }
}
