using Moq;
using BankEase.Application.DTOs;
using BankEase.Application.Services;
using BankEase.Core.Entities;
using BankEase.Infrastructure;

namespace BankEase.Tests.Services
{
    public class InterestRuleServiceTests
    {
        private InterestRuleService _interestRuleService;
        private Mock<IInterestRuleRepository> _mockInterestRuleRepository;

        [SetUp]
        public void Setup()
        {
            // Mock the IInterestRuleRepository
            _mockInterestRuleRepository = new Mock<IInterestRuleRepository>();

            // Initialize the InterestRuleService with the mocked repository
            _interestRuleService = new InterestRuleService(_mockInterestRuleRepository.Object);
        }

        [Test]
        public void AddOrUpdateInterestRule_ValidRule_AddsOrUpdatesRule()
        {
            // Arrange
            var inputDto = new InterestRuleInputDto
            {
                Date = DateTime.ParseExact("20230615", "yyyyMMdd", null),
                RuleId = "RULE03",
                Rate = 2.20m
            };

            // Act: Call the method under test
            _interestRuleService.AddOrUpdateInterestRule(inputDto);

            // Assert that AddOrUpdateRule was called once with the correct rule
            _mockInterestRuleRepository
                .Verify(r => r.AddOrUpdateRule(It.Is<InterestRule>(ir =>
                    ir.Date == inputDto.Date &&
                    ir.RuleId == inputDto.RuleId &&
                    ir.Rate == inputDto.Rate)), Times.Once);
        }

        [Test]
        public void AddOrUpdateInterestRule_InvalidRate_ThrowsException()
        {
            // Arrange
            var inputDto = new InterestRuleInputDto
            {
                Date = DateTime.ParseExact("20230615", "yyyyMMdd", null),
                RuleId = "RULE03",
                Rate = 101.00m // Invalid rate
            };

            // Act and Assert: Ensure an exception is thrown for invalid rate
            var ex = Assert.Throws<ArgumentException>(() =>
                _interestRuleService.AddOrUpdateInterestRule(inputDto));

            // Assert the exception message
            Assert.AreEqual("Interest rate must be greater than 0 and less than 100.", ex.Message);

            // Verify that the repository method was not called
            _mockInterestRuleRepository
                .Verify(r => r.AddOrUpdateRule(It.IsAny<InterestRule>()), Times.Never);
        }

        [Test]
        public void AddOrUpdateInterestRule_RateIsZero_ThrowsException()
        {
            // Arrange
            var inputDto = new InterestRuleInputDto
            {
                Date = DateTime.ParseExact("20230615", "yyyyMMdd", null),
                RuleId = "RULE03",
                Rate = 0.00m // Invalid rate
            };

            // Act and Assert: Ensure an exception is thrown for invalid rate
            var ex = Assert.Throws<ArgumentException>(() =>
                _interestRuleService.AddOrUpdateInterestRule(inputDto));

            // Assert the exception message
            Assert.AreEqual("Interest rate must be greater than 0 and less than 100.", ex.Message);

            // Verify that the repository method was not called
            _mockInterestRuleRepository
                .Verify(r => r.AddOrUpdateRule(It.IsAny<InterestRule>()), Times.Never);
        }

        [Test]
        public void AddOrUpdateInterestRule_RateIs100_ThrowsException()
        {
            // Arrange
            var inputDto = new InterestRuleInputDto
            {
                Date = DateTime.ParseExact("20230615", "yyyyMMdd", null),
                RuleId = "RULE03",
                Rate = 100.00m // Invalid rate
            };

            // Act and Assert: Ensure an exception is thrown for invalid rate
            var ex = Assert.Throws<ArgumentException>(() =>
                _interestRuleService.AddOrUpdateInterestRule(inputDto));

            // Assert the exception message
            Assert.AreEqual("Interest rate must be greater than 0 and less than 100.", ex.Message);

            // Verify that the repository method was not called
            _mockInterestRuleRepository
                .Verify(r => r.AddOrUpdateRule(It.IsAny<InterestRule>()), Times.Never);
        }

        [Test]
        public void AddOrUpdateInterestRule_NullOrEmptyRuleId_ThrowsException()
        {
            // Arrange
            var inputDto = new InterestRuleInputDto
            {
                Date = DateTime.ParseExact("20230615", "yyyyMMdd", null),
                RuleId = "", // Invalid RuleId
                Rate = 2.20m
            };

            // Act and Assert: Ensure an exception is thrown for invalid RuleId
            var ex = Assert.Throws<ArgumentException>(() =>
                _interestRuleService.AddOrUpdateInterestRule(inputDto));

            // Assert the exception message
            Assert.AreEqual("Rule ID cannot be null or empty.", ex.Message);

            // Verify that the repository method was not called
            _mockInterestRuleRepository
                .Verify(r => r.AddOrUpdateRule(It.IsAny<InterestRule>()), Times.Never);
        }
    }
}