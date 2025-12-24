using System.ComponentModel.DataAnnotations;

namespace BankApp.Api.DTOs
{
    public class CreateAccountRequest
    {
        [Required(ErrorMessage = "Account type is required.")]
        public string AccountType { get; set; }

        [Required(ErrorMessage = "Owner name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Owner name must be between 2 and 100 characters.")]
        public string OwnerName { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Initial deposit cannot be negative.")]
        public decimal InitialDeposit { get; set; }
    }
}
