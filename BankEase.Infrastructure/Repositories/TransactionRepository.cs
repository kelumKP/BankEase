using BankEase.Core.Entities;
using Microsoft.Data.Sqlite;

namespace BankEase.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private string ConnectionString => "Data Source=" + Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
            "BankEase.Infrastructure", "Data", "BankEaseDB.db");

        public TransactionRepository()
        {
            InitializeDatabase();
        }

        public Task<List<Transaction>> GetAllTransactionsForAccount(string accountNumber)
        {
            var transactions = new List<Transaction>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
        SELECT TransactionId, Date AS [Date], Type AS [Type], Amount AS [Amount], EODBalance AS [Balance]
        FROM Transactions
        WHERE AccountNumber = @AccountNumber
        ORDER BY Date";

                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) // No need for async here
                    {
                        var transactionId = reader.GetString(reader.GetOrdinal("TransactionId"));
                        var date = DateTime.ParseExact(reader.GetString(reader.GetOrdinal("Date")), "yyyyMMdd", null);
                        var typeString = reader.GetString(reader.GetOrdinal("Type"));
                        var amount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                        var eodBalance = reader.GetDecimal(reader.GetOrdinal("Balance"));

                        // Ensure the type is set correctly
                        TransactionType type = typeString == "D" ? TransactionType.D : TransactionType.W;

                        // Pass the fetched TransactionId to the Transaction constructor
                        transactions.Add(new Transaction(transactionId, date, type, amount, eodBalance));
                    }
                }
            }

            return Task.FromResult(transactions); // Wrap the result in Task
        }




        public Task<List<Transaction>> GetAllTransactionsForAccountPeriod(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var transactions = new List<Transaction>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TransactionId, Date, Type, Amount, EODBalance 
                    FROM Transactions 
                    WHERE AccountNumber = @AccountNumber 
                    AND Date BETWEEN @StartDate AND @EndDate
                    ORDER BY Date";
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@EndDate", endDate.ToString("yyyyMMdd"));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) // No need for async here
                    {
                        var transactionId = reader.GetString(0);
                        var date = DateTime.ParseExact(reader.GetString(1), "yyyyMMdd", null);
                        var typeString = reader.GetString(2);
                        var amount = reader.GetDecimal(3);
                        var eodBalance = reader.GetDecimal(4);

                        TransactionType type = typeString == "D" ? TransactionType.D : TransactionType.W;

                        transactions.Add(new Transaction(transactionId, date, type, amount, eodBalance));
                    }
                }
            }

            return Task.FromResult(transactions); // Wrap the result in Task
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    TransactionId TEXT PRIMARY KEY,
                    AccountNumber TEXT,
                    Date TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Amount DECIMAL NOT NULL,
                    EODBalance DECIMAL NOT NULL, -- New column
                    FOREIGN KEY (AccountNumber) REFERENCES Accounts(AccountNumber)
                );";
                command.ExecuteNonQuery();
            }
        }

    }
}
