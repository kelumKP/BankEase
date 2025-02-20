using Microsoft.Extensions.DependencyInjection;
using BankEase.Application.Interfaces;
using BankEase.Application.Services;
using BankEase.Infrastructure.Repositories;
using BankEase.Infrastructure;


namespace BankEase.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAwesomeGICBankServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<IAccountRepository, AccountRepository>();
            services.AddSingleton<ITransactionRepository, TransactionRepository>();
            services.AddSingleton<IInterestRuleRepository, InterestRuleRepository>();

            // Register application services
            services.AddSingleton<IBankingService, BankingService>();
            services.AddSingleton<IInterestRuleService, InterestRuleService>();

            // Register the top-level service
            services.AddSingleton<BankingApplicationService>();

            return services;
        }
    }
}
