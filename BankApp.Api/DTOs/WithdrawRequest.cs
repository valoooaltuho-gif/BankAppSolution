using System.ComponentModel.DataAnnotations;

namespace BankApp.Api.DTOs
{
    public class WithdrawRequest
    {
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Withdrawal amount must be positive.")]
        public decimal Amount { get; set; }
    }
}
