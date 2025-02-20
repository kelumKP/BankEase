namespace BankEase.Core.Entities
{

    public class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; private set; }
        public List<Transaction> Transactions { get; } = new List<Transaction>();

        public Account(string accountNumber)
        {
            AccountNumber = accountNumber;
            Balance = 0;
        }

        // Add a method to set the balance (can be used when initializing from DB)
        public void SetBalance(decimal balance)
        {
            Balance = balance;
        }

        public void Deposit(decimal amount, DateTime date)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be greater than zero.");

            Balance += amount;
            Transactions.Add(new Transaction(null, date, TransactionType.D, amount, Balance));
        }

        public void Withdraw(decimal amount, DateTime date)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be greater than zero.");

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            Balance -= amount;
            Transactions.Add(new Transaction(null, date, TransactionType.W, amount, Balance));
        }
    }
}
