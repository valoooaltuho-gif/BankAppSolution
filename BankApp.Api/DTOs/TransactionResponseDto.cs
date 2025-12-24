using System;
using BankApp.Core.Models;

namespace BankApp.Api.DTOs
{
    public class TransactionResponseDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; } // Оставим, так как это удобно для выписки
        public string Description { get; set; }
    }
}
