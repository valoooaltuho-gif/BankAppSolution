using System;

namespace BankApp.Core.Models
{
    public class InvalidAmountException : Exception
    {
        public InvalidAmountException() : base("Amount must be positive and non-zero.") { }

        public InvalidAmountException(string message) : base(message) { }

        public InvalidAmountException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
