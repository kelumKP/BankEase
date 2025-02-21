using BankEase.Core.Entities;

namespace BankEase.Infrastructure.Repositories
{
    public class TransactionRepository : BaseRepository, ITransactionRepository
    {
        public TransactionRepository()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS Transactions (
                    TransactionId TEXT PRIMARY KEY,
                    AccountNumber TEXT,
                    Date TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Amount DECIMAL NOT NULL,
                    EODBalance DECIMAL NOT NULL,
                    FOREIGN KEY (AccountNumber) REFERENCES Accounts(AccountNumber)
                );");
        }

        public Task<List<Transaction>> GetAllTransactionsForAccount(string accountNumber)
        {
            var transactions = ExecuteReader(@"
                SELECT TransactionId, Date, Type, Amount, EODBalance 
                FROM Transactions 
                WHERE AccountNumber = @AccountNumber 
                ORDER BY Date",
                reader => new Transaction(
                    reader.GetString(0),
                    DateTime.ParseExact(reader.GetString(1), "yyyyMMdd", null),
                    reader.GetString(2) == "D" ? TransactionType.D : TransactionType.W,
                    reader.GetDecimal(3),
                    reader.GetDecimal(4)),
                command => command.Parameters.AddWithValue("@AccountNumber", accountNumber));

            return Task.FromResult(transactions);
        }

        public Task<List<Transaction>> GetAllTransactionsForAccountPeriod(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var transactions = ExecuteReader(@"
                SELECT TransactionId, Date, Type, Amount, EODBalance 
                FROM Transactions 
                WHERE AccountNumber = @AccountNumber 
                AND Date BETWEEN @StartDate AND @EndDate
                ORDER BY Date",
                reader => new Transaction(
                    reader.GetString(0),
                    DateTime.ParseExact(reader.GetString(1), "yyyyMMdd", null),
                    reader.GetString(2) == "D" ? TransactionType.D : TransactionType.W,
                    reader.GetDecimal(3),
                    reader.GetDecimal(4)),
                command =>
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyyMMdd"));
                    command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyyMMdd"));
                });

            return Task.FromResult(transactions);
        }
    }
}