using Moq;
using BankEase.Core.Entities;
using BankEase.Infrastructure;
using BankEase.Application.DTOs;
using BankEase.Application.Services;

namespace BankEase.Tests.Services
{
    [TestFixture]
    public class BankingServiceTests
    {
        private Mock<IAccountRepository> _mockAccountRepository;
        private Mock<IInterestRuleRepository> _mockInterestRuleRepository;
        private Mock<ITransactionRepository> _mockTransactionRepository;
        private BankingService _bankingService;

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockInterestRuleRepository = new Mock<IInterestRuleRepository>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();

            // Initialize the service with mocks
            _bankingService = new BankingService(
                _mockAccountRepository.Object,
                _mockInterestRuleRepository.Object,
                _mockTransactionRepository.Object
            );
        }

        [Test]
        public void ProcessTransaction_ValidDeposit_UpdatesAccountBalanceAndCallsRepository()
        {
            // Arrange
            string accountNumber = "12345";
            DateTime transactionDate = DateTime.Now;
            decimal depositAmount = 100;
            var account = new Account(accountNumber);
            _mockAccountRepository.Setup(repo => repo.FindOrCreateAccount(accountNumber)).Returns(account);

            var transactionDto = new TransactionInputDto
            {
                AccountNumber = accountNumber,
                Date = transactionDate,
                Type = TransactionType.D,
                Amount = depositAmount
            };

            // Act
            _bankingService.ProcessTransaction(transactionDto);

            // Assert
            Assert.AreEqual(100, account.Balance);
            _mockAccountRepository.Verify(repo => repo.AddTransaction(accountNumber, transactionDate, TransactionType.D, depositAmount), Times.Once);
        }

        [Test]
        public void ProcessTransaction_ValidWithdrawal_UpdatesAccountBalanceAndCallsRepository()
        {
            // Arrange
            string accountNumber = "12345";
            DateTime transactionDate = DateTime.Now;
            decimal depositAmount = 100;
            decimal withdrawAmount = 50;
            var account = new Account(accountNumber);
            account.Deposit(depositAmount, transactionDate);
            _mockAccountRepository.Setup(repo => repo.FindOrCreateAccount(accountNumber)).Returns(account);

            var transactionDto = new TransactionInputDto
            {
                AccountNumber = accountNumber,
                Date = transactionDate,
                Type = TransactionType.W,
                Amount = withdrawAmount
            };

            // Act
            _bankingService.ProcessTransaction(transactionDto);

            // Assert
            Assert.AreEqual(50, account.Balance);
            _mockAccountRepository.Verify(repo => repo.AddTransaction(accountNumber, transactionDate, TransactionType.W, withdrawAmount), Times.Once);
        }

        [Test]
        public void ProcessTransaction_InsufficientBalance_ThrowsException()
        {
            // Arrange
            string accountNumber = "12345";
            DateTime transactionDate = DateTime.Now;
            decimal withdrawAmount = 100;
            var account = new Account(accountNumber);
            _mockAccountRepository.Setup(repo => repo.FindOrCreateAccount(accountNumber)).Returns(account);

            var transactionDto = new TransactionInputDto
            {
                AccountNumber = accountNumber,
                Date = transactionDate,
                Type = TransactionType.W,
                Amount = withdrawAmount
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _bankingService.ProcessTransaction(transactionDto));

            Assert.AreEqual("Insufficient balance.", exception.Message);

            // Verify that the repository method was not called
            _mockAccountRepository.Verify(repo => repo.AddTransaction(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TransactionType>(), It.IsAny<decimal>()), Times.Never);
        }

        [Test]
        public void ProcessTransaction_NegativeAmount_ThrowsException()
        {
            // Arrange
            string accountNumber = "12345";
            DateTime transactionDate = DateTime.Now;
            decimal negativeAmount = -50;
            var account = new Account(accountNumber);
            _mockAccountRepository.Setup(repo => repo.FindOrCreateAccount(accountNumber)).Returns(account);

            var transactionDto = new TransactionInputDto
            {
                AccountNumber = accountNumber,
                Date = transactionDate,
                Type = TransactionType.D,
                Amount = negativeAmount
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _bankingService.ProcessTransaction(transactionDto));

            Assert.AreEqual("Transaction amount must be greater than zero.", exception.Message);

            // Verify that the repository method was not called
            _mockAccountRepository.Verify(repo => repo.AddTransaction(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<TransactionType>(), It.IsAny<decimal>()), Times.Never);
        }

        [Test]
        public async Task GetTransactionsForAccount_ValidAccount_ReturnsTransactions()
        {
            // Arrange
            string accountNumber = "12345";
            var transactions = new List<Transaction>
            {
                new Transaction("txn1", DateTime.Now, TransactionType.D, 100, 100),
                new Transaction("txn2", DateTime.Now, TransactionType.W, 50, 50)
            };

            _mockTransactionRepository.Setup(repo => repo.GetAllTransactionsForAccount(accountNumber))
                .ReturnsAsync(transactions);

            // Act
            var result = await _bankingService.GetTransactionsForAccount(accountNumber);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("txn1", result[0].TransactionId);
            Assert.AreEqual("txn2", result[1].TransactionId);
        }

        [Test]
        public async Task GetTransactionsForAccount_NoTransactions_ReturnsEmptyList()
        {
            // Arrange
            string accountNumber = "12345";
            var transactions = new List<Transaction>();

            _mockTransactionRepository.Setup(repo => repo.GetAllTransactionsForAccount(accountNumber))
                .ReturnsAsync(transactions);

            // Act
            var result = await _bankingService.GetTransactionsForAccount(accountNumber);

            // Assert
            Assert.IsEmpty(result);
        }
    }
}