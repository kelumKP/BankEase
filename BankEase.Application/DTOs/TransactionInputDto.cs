using BankEase.Core.Entities;


namespace BankEase.Application.DTOs
{
    public class TransactionInputDto
    {
        public DateTime Date { get; set; }
        public string AccountNumber { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
    }
}
