using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core;
using BankApp.Core.Models;
using BankApp.Infrastructure;

namespace BankApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly BankContext _context; // !!! Добавьте контекст БД !!!

        public IAccountRepository Repository { get; }

        // Внедрение зависимости (Dependency Injection) через конструктор
        public AccountService(IAccountRepository accountRepository, BankContext context)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context)); // !!! Инициализируйте контекст !!!
        }

        public AccountService(IAccountRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _accountRepository.GetAllAsync();
        }

        public async Task<Account> CreateAccountAsync(string accountType, string ownerName, decimal initialDeposit)
        {
            Account newAccount = AccountFactory.CreateAccount(accountType, ownerName, initialDeposit);


            await _accountRepository.AddAsync(newAccount);
            return newAccount;
        }

        public async Task DepositAsync(Guid accountId, decimal amount)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new KeyNotFoundException($"Account {accountId} not found.");

            // Логика инкапсулирована в доменном объекте Account
            account.Deposit(amount);

            // Сохраняем обновленное состояние в репозитории
            await _accountRepository.UpdateAsync(account);
        }

        public async Task WithdrawAsync(Guid accountId, decimal amount)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new KeyNotFoundException($"Account {accountId} not found.");

            // Логика инкапсулирована в доменном объекте Account
            account.Withdraw(amount);

            // Сохраняем обновленное состояние в репозитории
            await _accountRepository.UpdateAsync(account);
        }

        public async Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            if (amount <= 0) throw new InvalidAmountException("Transfer amount must be positive.");

            // --- НАЧАЛО ЯВНОЙ ТРАНЗАКЦИИ БД ---
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Получаем оба аккаунта внутри транзакции
                var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
                var toAccount = await _accountRepository.GetByIdAsync(toAccountId);

                if (fromAccount == null) throw new KeyNotFoundException($"Source account {fromAccountId} not found.");
                if (toAccount == null) throw new KeyNotFoundException($"Destination account {toAccountId} not found.");

                // Логика домена
                fromAccount.Withdraw(amount);
                toAccount.Deposit(amount);

                // Сохраняем ОБА аккаунта (репозиторий использует SaveChangesAsync, который работает в контексте текущей транзакции)
                await _accountRepository.UpdateAsync(fromAccount);
                await _accountRepository.UpdateAsync(toAccount);

                // --- ПОДТВЕРЖДЕНИЕ ТРАНЗАКЦИИ (COMMIT) ---
                await transaction.CommitAsync();
            }
            catch
            {
                // --- ОТКАТ ТРАНЗАКЦИИ (ROLLBACK) при любой ошибке ---
                await transaction.RollbackAsync();
                throw; // Повторно выбрасываем исходную ошибку
            }
        }

        public async Task<IEnumerable<Transaction>> GetStatementAsync(Guid accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) throw new KeyNotFoundException($"Account {accountId} not found.");

            return account.Transactions;
        }

        public async Task RunMonthlyProcessAsync()
        {
            Console.WriteLine("Starting monthly process: applying fees and interest.");
            var accounts = await _accountRepository.GetAllAsync();
            int processedCount = 0;

            foreach (var account in accounts)
            {
                bool updated = false;

                // Проверяем, является ли счет сберегательным (SavingsAccount)
                if (account is SavingsAccount savings)
                {
                    savings.ApplyInterest();
                    updated = true;
                    Console.WriteLine($"Applied interest to Savings Account {account.Id}");
                }

                // Проверяем, является ли счет бизнес-счетом (BusinessAccount)
                if (account is BusinessAccount business)
                {
                    business.ApplyMonthlyFee();
                    updated = true;
                    Console.WriteLine($"Applied monthly fee to Business Account {account.Id}");
                }

                // Если счет был обновлен, сохраняем его
                if (updated)
                {
                    await _accountRepository.UpdateAsync(account);
                    processedCount++;
                }
            }
            Console.WriteLine($"Monthly process completed. {processedCount} accounts updated.");
        }

        public async Task<Account> GetByIdAsync(Guid id)
        {
            Console.WriteLine($"Fetching account details for AccountId: {id}");

            // Используем репозиторий для получения объекта
            var account = await _accountRepository.GetByIdAsync(id);

            if (account == null)
            {
                Console.WriteLine($"Account not found for AccountId: {id}");
                // В зависимости от требований, можно вернуть null или выбросить исключение KeyNotFoundException
                // Для API мы обычно возвращаем null, чтобы контроллер обработал 404 Not Found
                return null;
            }

            return account;
        }
    }
}
