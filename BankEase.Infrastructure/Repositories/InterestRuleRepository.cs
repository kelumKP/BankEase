using BankEase.Core.Entities;

namespace BankEase.Infrastructure.Repositories
{
    public class InterestRuleRepository : BaseRepository, IInterestRuleRepository
    {
        public InterestRuleRepository()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS InterestRules (
                    Date TEXT PRIMARY KEY,
                    RuleId TEXT NOT NULL,
                    Rate DECIMAL NOT NULL
                );");
        }

        public void AddOrUpdateRule(InterestRule rule)
        {
            ExecuteNonQuery(@"
                INSERT OR REPLACE INTO InterestRules (Date, RuleId, Rate)
                VALUES (@Date, @RuleId, @Rate)",
                command =>
                {
                    command.Parameters.AddWithValue("@Date", rule.Date.ToString("yyyyMMdd"));
                    command.Parameters.AddWithValue("@RuleId", rule.RuleId);
                    command.Parameters.AddWithValue("@Rate", rule.Rate);
                });
        }

        public List<InterestRule> GetAllRules()
        {
            return ExecuteReader("SELECT Date, RuleId, Rate FROM InterestRules ORDER BY Date",
                reader => new InterestRule(
                    DateTime.ParseExact(reader.GetString(0), "yyyyMMdd", null),
                    reader.GetString(1),
                    reader.GetDecimal(2)));
        }
    }
}