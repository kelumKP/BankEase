using BankEase.Application.DTOs;
using BankEase.Application.Interfaces;
using BankEase.Core.Entities;

namespace BankEase.Application
{
    public class BankingApplicationService
    {
        private readonly IBankingService _bankingService;
        private readonly IInterestRuleService _interestRuleService;

        // Inject dependencies via constructor
        public BankingApplicationService(IBankingService bankingService, IInterestRuleService interestRuleService)
        {
            _bankingService = bankingService;
            _interestRuleService = interestRuleService;
        }

        public void Start()
        {
            while (true)
            {
                Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
                Console.WriteLine("[T] Input transactions");
                Console.WriteLine("[I] Define interest rules");
                Console.WriteLine("[P] Print statement");
                Console.WriteLine("[Q] Quit");
                Console.Write("> ");

                var input = Console.ReadLine()?.ToUpper();

                switch (input)
                {
                    case "T":
                        InputTransaction();
                        break;
                    case "I":
                        DefineInterestRule();
                        break;
                    case "P":
                        PrintStatement().Wait();
                        break;
                    case "Q":
                        Quit();
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private void InputTransaction()
        {
            Console.WriteLine("Enter transaction details <Date YYYYMMDD> <Account> <D/W> <Amount>:");
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) return;

            var parts = input.Split(' ');
            if (parts.Length != 4)
            {
                Console.WriteLine("Invalid input format.");
                return;
            }

            try
            {
                var transactionDto = new TransactionInputDto
                {
                    Date = DateTime.ParseExact(parts[0], "yyyyMMdd", null),
                    AccountNumber = parts[1],
                    Type = parts[2].ToUpper() == "D" ? TransactionType.D : TransactionType.W,
                    Amount = decimal.Parse(parts[3])
                };

                // Process the transaction
                _bankingService.ProcessTransaction(transactionDto);
                Console.WriteLine("Transaction processed successfully.");

                // Fetch the updated list of transactions for the account
                var transactions = _bankingService.GetAccountTransactions(transactionDto.AccountNumber);

                // Display the transactions in the desired format
                Console.WriteLine($"Account: {transactionDto.AccountNumber}");
                Console.WriteLine("| Date     | Txn Id      | Type | Amount |");

                foreach (var txn in transactions)
                {
                    Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TransactionId} | {txn.Type} | {txn.Amount,7:F2} |");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void DefineInterestRule()
        {
            Console.WriteLine("Enter interest rule <Date YYYYMMDD> <RuleId> <Rate %>:");
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) return;

            var parts = input.Split(' ');
            if (parts.Length != 3)
            {
                Console.WriteLine("Invalid input format.");
                return;
            }

            try
            {
                var interestRuleDto = new InterestRuleInputDto
                {
                    Date = DateTime.ParseExact(parts[0], "yyyyMMdd", null),
                    RuleId = parts[1],
                    Rate = decimal.Parse(parts[2])
                };

                // Add or update the interest rule
                _interestRuleService.AddOrUpdateInterestRule(interestRuleDto);
                Console.WriteLine("Interest rule added/updated successfully.");

                // Fetch the updated list of interest rules
                var interestRules = _interestRuleService.GetAllInterestRules();

                // Display the interest rules in the desired format
                Console.WriteLine("Interest rules:");
                Console.WriteLine("| Date     | RuleId | Rate (%) |");

                foreach (var rule in interestRules)
                {
                    Console.WriteLine($"| {rule.Date:yyyyMMdd} | {rule.RuleId} | {rule.Rate,7:F2} |");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task PrintStatement()
        {
            Console.WriteLine("Enter account and statement period <Account> <YYYYMM>:");
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) return;

            var parts = input.Split(' ');
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid input format.");
                return;
            }

            try
            {
                var statementDto = new StatementRequestDto
                {
                    AccountNumber = parts[0],
                    Year = int.Parse(parts[1].Substring(0, 4)),
                    Month = int.Parse(parts[1].Substring(4, 2))
                };

                var transactions = await _bankingService.GetTransactionsForAccount(statementDto.AccountNumber);
                var interest = await _bankingService.CalculateInterest(statementDto.AccountNumber, statementDto.Year, statementDto.Month);

                Console.WriteLine($"Statement for Account: {statementDto.AccountNumber}");
                Console.WriteLine("| Date     | Txn Id      | Type | Amount | Balance |");

                decimal balance = 0;
                foreach (var txn in transactions)
                {
                    balance += txn.Type == TransactionType.D ? txn.Amount : -txn.Amount;
                    Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TransactionId} | {txn.Type} | {txn.Amount,7:F2} | {balance,7:F2} |");
                }

                if (interest > 0.0m)
                {
                    balance += interest;
                    Console.WriteLine($"| {new DateTime(statementDto.Year, statementDto.Month, DateTime.DaysInMonth(statementDto.Year, statementDto.Month)):yyyyMMdd} |             | I    | {interest,7:F2} | {balance,7:F2} |");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void Quit()
        {
            Console.WriteLine("Thank you for banking with AwesomeGIC Bank.");
            Console.WriteLine("Have a nice day!");
        }
    }
}