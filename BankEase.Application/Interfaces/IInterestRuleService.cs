using BankEase.Core.Entities;
using BankEase.Application.DTOs;

namespace BankEase.Application.Interfaces
{
    public interface IInterestRuleService
    {
        void AddOrUpdateInterestRule(InterestRuleInputDto inputDto);
        List<InterestRule> GetAllInterestRules();
    }
}
