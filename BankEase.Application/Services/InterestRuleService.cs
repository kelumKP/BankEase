using BankEase.Application.DTOs;
using BankEase.Application.Interfaces;
using BankEase.Core.Entities;
using BankEase.Infrastructure;

namespace BankEase.Application.Services
{
    public class InterestRuleService : IInterestRuleService
    {
        private readonly IInterestRuleRepository _interestRuleRepository;

        public InterestRuleService(IInterestRuleRepository interestRuleRepository)
        {
            _interestRuleRepository = interestRuleRepository;
        }

        public void AddOrUpdateInterestRule(InterestRuleInputDto inputDto)
        {
            if (string.IsNullOrWhiteSpace(inputDto.RuleId))
                throw new ArgumentException("Rule ID cannot be null or empty.");

            if (inputDto.Rate <= 0 || inputDto.Rate >= 100)
                throw new ArgumentException("Interest rate must be greater than 0 and less than 100.");

            var rule = new InterestRule(inputDto.Date, inputDto.RuleId, inputDto.Rate);
            _interestRuleRepository.AddOrUpdateRule(rule);
        }

        public List<InterestRule> GetAllInterestRules()
        {
            return _interestRuleRepository.GetAllRules()
                                          .OrderBy(r => r.Date)
                                          .ToList();
        }
    }
}