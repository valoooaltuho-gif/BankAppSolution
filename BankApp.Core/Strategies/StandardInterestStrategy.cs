namespace BankApp.Core.Models
{
    public class StandardInterestStrategy : IInterestCalculationStrategy
    {
        private const decimal InterestRate = 0.02m; // 2%

        public decimal CalculateInterest(decimal balance)
        {
            if (balance > 0)
            {
                return balance * InterestRate;
            }
            return 0m;
        }
    }
}
