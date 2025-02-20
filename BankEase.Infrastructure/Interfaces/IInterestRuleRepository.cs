using BankEase.Core.Entities;

namespace BankEase.Infrastructure
{
    public interface IInterestRuleRepository
    {
        void AddOrUpdateRule(InterestRule rule);
        List<InterestRule> GetAllRules();
    }
}
