using System;
using System.Collections.Generic;

namespace BankApp.Api.DTOs
{
    public class AccountResponseDto
    {
        public Guid Id { get; set; }
        public string OwnerName { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AccountType { get; set; } // Добавим тип аккаунта явно
        public List<TransactionResponseDto> Transactions { get; set; }
    }
}
