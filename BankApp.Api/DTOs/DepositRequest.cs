using System.ComponentModel.DataAnnotations;

namespace BankApp.Api.DTOs
{
    public class DepositRequest
    {
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
        public decimal Amount { get; set; }
    }
}
