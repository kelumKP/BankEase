namespace BankEase.Application.DTOs
{
    public class InterestRuleInputDto
    {
        public DateTime Date { get; set; }
        public string RuleId { get; set; }
        public decimal Rate { get; set; }
    }
}
