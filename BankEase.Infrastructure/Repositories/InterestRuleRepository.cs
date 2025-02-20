using BankEase.Core.Entities;
using Microsoft.Data.Sqlite;

namespace BankEase.Infrastructure.Repositories
{
    public class InterestRuleRepository : IInterestRuleRepository
    {
        private string ConnectionString => "Data Source=" + Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
            "BankEase.Infrastructure", "Data", "BankEaseDB.db");

        public InterestRuleRepository()
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
                CREATE TABLE IF NOT EXISTS InterestRules (
                    Date TEXT PRIMARY KEY,
                    RuleId TEXT NOT NULL,
                    Rate DECIMAL NOT NULL
                );";
                command.ExecuteNonQuery();
            }
        }

        public void AddOrUpdateRule(InterestRule rule)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT OR REPLACE INTO InterestRules (Date, RuleId, Rate)
                VALUES (@Date, @RuleId, @Rate)";
                command.Parameters.AddWithValue("@Date", rule.Date.ToString("yyyyMMdd"));
                command.Parameters.AddWithValue("@RuleId", rule.RuleId);
                command.Parameters.AddWithValue("@Rate", rule.Rate);
                command.ExecuteNonQuery();
            }
        }

        public List<InterestRule> GetAllRules()
        {
            var rules = new List<InterestRule>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Date, RuleId, Rate FROM InterestRules ORDER BY Date";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = DateTime.ParseExact(reader.GetString(0), "yyyyMMdd", null);
                        var ruleId = reader.GetString(1);
                        var rate = reader.GetDecimal(2);
                        rules.Add(new InterestRule(date, ruleId, rate));
                    }
                }
            }
            return rules;
        }
    }
}