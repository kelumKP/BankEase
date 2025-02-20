using BankEase.Core.Entities;

namespace BankEase.Infrastructure
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetAllTransactionsForAccount(string accountNumber);
        Task<List<Transaction>> GetAllTransactionsForAccountPeriod(string accountNumber, DateTime startDate, DateTime endDate);
    }
}
