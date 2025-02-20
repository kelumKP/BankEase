using Microsoft.Extensions.DependencyInjection;
using BankEase.Application;

class Program
{
    static void Main(string[] args)
    {
        // Set up the DI container using the extension method
        var serviceProvider = new ServiceCollection()
            .AddBankEaseServices() // Call the extension method to register all dependencies
            .BuildServiceProvider();

        // Resolve the BankingApplicationService from the DI container
        var bankingAppService = serviceProvider.GetService<BankingApplicationService>();
        bankingAppService.Start();
    }
}