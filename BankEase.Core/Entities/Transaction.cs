namespace BankEase.Core.Entities
{
    public class Transaction
    {
        public DateTime Date { get; }
        public string TransactionId { get; }
        public TransactionType Type { get; }
        public decimal Amount { get; }
        public decimal EODBalance { get; }

        // Modify constructor to accept TransactionId as parameter
        public Transaction(string transactionId, DateTime date, TransactionType type, decimal amount, decimal eodBalance)
        {
            TransactionId = transactionId;
            Date = date;
            Type = type;
            Amount = amount;
            EODBalance = eodBalance;
        }
    }




    public enum TransactionType
    {
        // "D" for Deposit or "W" for Withdrawal
        D,
        W
    }
}
