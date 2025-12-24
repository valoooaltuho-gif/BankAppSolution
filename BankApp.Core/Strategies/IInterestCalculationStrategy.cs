// В BankApp.Core/IInterestCalculationStrategy.cs

namespace BankApp.Core.Models
{
    public interface IInterestCalculationStrategy
    {
        decimal CalculateInterest(decimal balance);
    }
}
