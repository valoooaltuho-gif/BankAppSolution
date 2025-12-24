using System;

namespace BankApp.Core.Models
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException() : base("Insufficient funds to perform the operation.") { }

        public InsufficientFundsException(string message) : base(message) { }

        public InsufficientFundsException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}