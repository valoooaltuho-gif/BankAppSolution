// BankApp.Core/Transaction.cs (ВОССТАНОВЛЕННЫЙ код)

using System;

namespace BankApp.Core.Models
{
    public record Transaction(
        Guid Id,
        DateTime Date,
        TransactionType Type,
        decimal Amount,
        decimal BalanceAfter, // ВОССТАНОВЛЕНО
        string Description
    );
}
