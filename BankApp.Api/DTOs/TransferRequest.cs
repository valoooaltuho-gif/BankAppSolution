using System;
using System.ComponentModel.DataAnnotations;

namespace BankApp.Api.DTOs
{
    public class TransferRequest
    {
        [Required(ErrorMessage = "Destination account ID is required.")]
        public Guid ToAccountId { get; set; }

        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Transfer amount must be positive.")]
        public decimal Amount { get; set; }
    }
}
