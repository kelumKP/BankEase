namespace BankEase.Application.DTOs
{
    public class StatementRequestDto
    {
        public string AccountNumber { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
