using BankEase.Core.Entities;
using Microsoft.Data.Sqlite;
using System.IO; 

namespace BankEase.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {


        private string ConnectionString => "Data Source=" + Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
            "BankEase.Infrastructure", "Data", "BankEaseDB.db");

      
        public AccountRepository()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Accounts (
            AccountNumber TEXT PRIMARY KEY,
            Balance DECIMAL NOT NULL
        );
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

        public Account FindAccount(string accountNumber)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT AccountNumber, Balance FROM Accounts WHERE AccountNumber = @AccountNumber";
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var account = new Account(reader.GetString(0));
                        account.SetBalance(reader.GetDecimal(1)); // Set initial balance using SetBalance
                        return account;
                    }
                }
            }
            return null; // Return null if account not found
        }



        public Account FindOrCreateAccount(string accountNumber)
        {
            var account = FindAccount(accountNumber);
            if (account == null)
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Accounts (AccountNumber, Balance) VALUES (@AccountNumber, 0)";
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.ExecuteNonQuery();
                }
                account = new Account(accountNumber);
            }
            return account;
        }

        public void AddTransaction(string accountNumber, DateTime date, TransactionType type, decimal amount)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Ensure account exists before inserting transaction
                var checkAccountCommand = connection.CreateCommand();
                checkAccountCommand.CommandText = "SELECT Balance FROM Accounts WHERE AccountNumber = @AccountNumber";
                checkAccountCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                var currentBalance = Convert.ToDecimal(checkAccountCommand.ExecuteScalar());

                // Update the account balance based on the transaction type
                if (type == TransactionType.D)
                {
                    currentBalance += amount; // Add the amount for deposits
                }
                else if (type == TransactionType.W)
                {
                    currentBalance -= amount; // Subtract the amount for withdrawals
                }

                // Update the account balance in the database
                var updateBalanceCommand = connection.CreateCommand();
                updateBalanceCommand.CommandText = "UPDATE Accounts SET Balance = @Balance WHERE AccountNumber = @AccountNumber";
                updateBalanceCommand.Parameters.AddWithValue("@Balance", currentBalance);
                updateBalanceCommand.Parameters.AddWithValue("@AccountNumber", accountNumber);
                updateBalanceCommand.ExecuteNonQuery();

                // Insert the transaction
                var transactionId = $"{date:yyyyMMdd}-{GetNextTransactionNumber(accountNumber, date):00}";

                var command = connection.CreateCommand();
                command.CommandText = @"
        INSERT INTO Transactions (TransactionId, AccountNumber, Date, Type, Amount, EODBalance)
        VALUES (@TransactionId, @AccountNumber, @Date, @Type, @Amount, @EODBalance)";
                command.Parameters.AddWithValue("@TransactionId", transactionId);
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@Date", date.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@Type", type.ToString());
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@EODBalance", currentBalance); // Add EODBalance

                command.ExecuteNonQuery();
            }
        }


        private int GetNextTransactionNumber(string accountNumber, DateTime date)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT COUNT(*) 
                FROM Transactions 
                WHERE AccountNumber = @AccountNumber AND Date = @Date";
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@Date", date.ToString("yyyyMMdd"));

                var count = Convert.ToInt32(command.ExecuteScalar());
                return count + 1;
            }
        }

        public List<Transaction> GetTransactionsForAccount(string accountNumber)
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
            ORDER BY Date";
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var transactionId = reader.GetString(0);
                        var date = DateTime.ParseExact(reader.GetString(1), "yyyyMMdd", null);
                        var typeString = reader.GetString(2); // "D" for Deposit or "W" for Withdrawal
                        var amount = reader.GetDecimal(3);
                        var eodBalance = reader.GetDecimal(4); // EODBalance from the database

                        // Convert the string type to TransactionType enum
                        TransactionType type = typeString == "D" ? TransactionType.D : TransactionType.W;

                        transactions.Add(new Transaction(transactionId, date, type, amount, eodBalance));
                    }
                }
            }
            return transactions;
        }
    }
}