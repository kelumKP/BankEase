using BankEase.Core.Entities;
using BankEase.Infrastructure.Repositories.BankEase.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace BankEase.Infrastructure.Repositories
{
    public class AccountRepository : BaseRepository, IAccountRepository
    {
        public AccountRepository()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            ExecuteNonQuery(@"
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
                    EODBalance DECIMAL NOT NULL,
                    FOREIGN KEY (AccountNumber) REFERENCES Accounts(AccountNumber)
                );");
        }

        public Account FindAccount(string accountNumber)
        {
            return ExecuteReader("SELECT AccountNumber, Balance FROM Accounts WHERE AccountNumber = @AccountNumber",
                reader =>
                {
                    var account = new Account(reader.GetString(0));
                    account.SetBalance(reader.GetDecimal(1));
                    return account;
                },
                command => command.Parameters.AddWithValue("@AccountNumber", accountNumber)).FirstOrDefault();
        }

        public Account FindOrCreateAccount(string accountNumber)
        {
            var account = FindAccount(accountNumber);
            if (account == null)
            {
                ExecuteNonQuery("INSERT INTO Accounts (AccountNumber, Balance) VALUES (@AccountNumber, 0)",
                    command => command.Parameters.AddWithValue("@AccountNumber", accountNumber));
                account = new Account(accountNumber);
            }
            return account;
        }

        public void AddTransaction(string accountNumber, DateTime date, TransactionType type, decimal amount)
        {
            var currentBalance = ExecuteScalar<decimal>("SELECT Balance FROM Accounts WHERE AccountNumber = @AccountNumber",
                command => command.Parameters.AddWithValue("@AccountNumber", accountNumber));

            currentBalance += type == TransactionType.D ? amount : -amount;

            ExecuteNonQuery("UPDATE Accounts SET Balance = @Balance WHERE AccountNumber = @AccountNumber",
                command =>
                {
                    command.Parameters.AddWithValue("@Balance", currentBalance);
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                });

            var transactionId = $"{date:yyyyMMdd}-{GetNextTransactionNumber(accountNumber, date):00}";

            ExecuteNonQuery(@"
                INSERT INTO Transactions (TransactionId, AccountNumber, Date, Type, Amount, EODBalance)
                VALUES (@TransactionId, @AccountNumber, @Date, @Type, @Amount, @EODBalance)",
                command =>
                {
                    command.Parameters.AddWithValue("@TransactionId", transactionId);
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.Parameters.AddWithValue("@Date", date.ToString("yyyyMMdd"));
                    command.Parameters.AddWithValue("@Type", type.ToString());
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@EODBalance", currentBalance);
                });
        }

        private int GetNextTransactionNumber(string accountNumber, DateTime date)
        {
            return ExecuteScalar<int>(@"
                SELECT COUNT(*) 
                FROM Transactions 
                WHERE AccountNumber = @AccountNumber AND Date = @Date",
                command =>
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.Parameters.AddWithValue("@Date", date.ToString("yyyyMMdd"));
                });
        }

        public List<Transaction> GetTransactionsForAccount(string accountNumber)
        {
            return ExecuteReader(@"
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
        }
    }
}