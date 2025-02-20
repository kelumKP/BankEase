using BankEase.Core.Entities;

namespace BankEase.Infrastructure
{
    public interface IAccountRepository
    {
        Account FindAccount(string accountNumber);
        Account FindOrCreateAccount(string accountNumber);
        void AddTransaction(string accountNumber, DateTime date, TransactionType type, decimal amount);
        List<Transaction> GetTransactionsForAccount(string accountNumber);
    }
}
