# BankEase Console Application

Welcome to the **BankEase Console Application**! This application is a simple banking system that allows users to manage transactions, define interest rules, and generate account statements. It is built using C# and leverages SQLite for data persistence.

## Features

1. **Input Transactions**:
   - Users can input transactions in the format `<Date> <Account> <Type> <Amount>`.
   - Supported transaction types are `D` (Deposit) and `W` (Withdrawal).
   - Transactions are processed and stored in the database.

2. **Define Interest Rules**:
   - Users can define interest rules in the format `<Date> <RuleId> <Rate in %>`.
   - Interest rules are applied to calculate interest for accounts.

3. **Print Statement**:
   - Users can generate a statement for a specific account and month.
   - The statement includes all transactions and the calculated interest for the month.

4. **Quit**:
   - Users can exit the application.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on the machine.
- SQLite (included in the project via `SQLitePCL`).

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/kelumkp/BankEase.git

2. Navigate to the project directory:
   ```bash
   cd BankEase/BankEase.ConsoleApp
3. Restore the NuGet packages:
   ```bash
   dotnet restore
4. Build the project:
   ```bash
   dotnet build
5. Run the application: 
   ```bash
   dotnet run

### Usage

1. **Input Transactions:**
   - Enter the transaction details in the format `<Date> <Account> <Type> <Amount>`.
   - Example: `20231001 ACC001 D 100.00`

2. **Define Interest Rules:**
   - Enter the interest rule details in the format `<Date> <RuleId> <Rate in %>`.
   - Example: `20231001 RULE01 1.95`

3. **Print Statement:**
   - Enter the account and month in the format `<Account> <Year><Month>`.
   - Example: `ACC001 202310`

4. **Quit:**
   - Enter `Q` to exit the application.

### Project Structure

- **BankEase.Presentation:** The main console application.
  - `Program.cs`: Entry point of the application.

- **BankEase.Application:** Contains the core business logic and Application logic.
  - `Services`: Business services like `BankingService` , `InterestRuleService` and Application services like `BankingApplicationService`.
  - `BankingApplicationService.cs` and `ServiceCollectionExtensions`

- **BankEase.Core:** Contains the core entities.
  - `Entities`: Domain entities like `Account`, `Transaction`, and `InterestRule`.

- **BankEase.Infrastructure:** Contains the data access layer.
  - `Repositories`: Repository implementations for base Initilization,`Account`, `Transaction`, and `InterestRule`.
  - `Interfaces`: Repository interfaces.

- **BankEase.Tests:** Unit tests for the application.
  - `Services`: Tests for `BankingService` and `InterestRuleService`.


## Components

###  **BankingApplicationService**
This is the main application service responsible for handling user interactions and coordinating the banking operations. It provides a console-based interface with the following options:
   - **Input transactions**: Allow the user to input deposit or withdrawal transactions.
   - **Define interest rules**: Let the user define or update interest rules for accounts.
   - **Print statement**: Generate and display a statement showing the account's transactions and calculated interest.

### **ServiceCollectionExtensions**
This file contains an extension method for `IServiceCollection`, used to configure and register all the necessary services and repositories into the Dependency Injection (DI) container. These include:
   - **Repositories**:
     - `AccountRepository`: Handles data access related to account information.
     - `TransactionRepository`: Manages transactions related to accounts.
     - `InterestRuleRepository`: Stores and manages interest rules.
   - **Application Services**:
     - `BankingService`: Handles banking operations such as processing transactions and calculating balances.
     - `InterestRuleService`: Manages the addition and retrieval of interest rules.
   - **Top-Level Service**:
     - `BankingApplicationService`: Provides the user interface and coordinates the banking operations.

## Usage

**BankingApplicationService** is the entry point for the application. It provides the main console interface where users can:
   - Input transactions (deposit/withdrawal),
   - Define or update interest rules,
   - Print statements showing account transactions and calculated interest.
   
**ServiceCollectionExtensions** is used to register all the necessary services into the DI container. It ensures loose coupling between different parts of the application and makes it easier to manage dependencies.


### Testing

The project includes unit tests for the core services. To run the tests:

1. Navigate to the test project:
   ```bash
   cd BankEase.Tests

2. Run the tests:

   ```bash
   dotnet test

### Database
The application uses SQLite for data persistence. The database file (`BankEase.db`) is automatically created in the project directory when the application is first run.

for an example in order to add first transaction for `AC001` bank account can use `20230626 AC001 D 100.00`


### Contributing
Contributions are welcome! Please fork the repository and submit a pull request with the changes.

### License
This project is licensed under the MIT License. See the LICENSE file for details.

### Acknowledgments
 - `SQLitePCL` for SQLite database access.
 - `Moq` for mocking in unit tests.
 - `NUnit` for unit testing.
	
Thank you for using BankEase! We hope you find this application useful. If you have any questions or feedback, please feel free to reach out.

---------------------------------------

# Bank Account

You're designing a simple banking system that handles operations on bank accounts. The system should be capable of the following features:
- input banking transactions
- calculate interest
- printing account statement

All the example below assumes console input/output is used. 


When launching the application, it prompts user for actions:
```
Welcome to AwesomeGIC Bank! What would you like to do?
[T] Input transactions 
[I] Define interest rules
[P] Print statement
[Q] Quit
>
```

User should be able to enter `T` or `t` to select input transactions menu. Similarly, initial character is used for other options.

## Input transactions
Upon selecting Input transactions option, application prompts user for transaction details.
```
Please enter transaction details in <Date> <Account> <Type> <Amount> format 
(or enter blank to go back to main menu):
>
```

User is then able to enter something like the following:
```
20230626 AC001 W 100.00
```
The system should automatically create the account when the first transaction for the account is created.

Some constraints to note:
* Date should be in YYYYMMdd format
* Account is a string, free format
* Type is D for deposit, W for withdrawal, case insensitive
* Amount must be greater than zero, decimals are allowed up to 2 decimal places
* An account's balance should not be less than 0. Therefore, the first transaction on an account should not be a withdrawal, and any transactions thereafter should not make balance go below 0
* Each transaction should be given a unique id in YYYMMdd-xx format, where xx is a running number (see example below)

Then system responds by displaying the statement of the account:
(assuming there are already some transactions in the account)
```
Account: AC001
| Date     | Txn Id      | Type | Amount |
| 20230505 | 20230505-01 | D    | 100.00 |
| 20230601 | 20230601-01 | D    | 150.00 |
| 20230626 | 20230626-01 | W    |  20.00 |
| 20230626 | 20230626-02 | W    | 100.00 |

Is there anything else you'd like to do?
[T] Input transactions 
[I] Define interest rules
[P] Print statement
[Q] Quit
>
```

## Define interest rule
Upon selecting Define interest rule option, application prompts user to define interest rules:

```
Please enter interest rules details in <Date> <RuleId> <Rate in %> format 
(or enter blank to go back to main menu):
>
```

User is then able to enter something like the following:
```
20230615 RULE03 2.20
```
Some constraints to note:
* Date should be in YYYYMMdd format
* RuleId is string, free format
* Interest rate should be greater than 0 and less than 100
* If there's any existing rules on the same day, the latest one is kept

Then system responds by listing all interest rules orderd by date:
(assuming there are already RULE01 and RULE02 in the system) 
```
Interest rules:
| Date     | RuleId | Rate (%) |
| 20230101 | RULE01 |     1.95 |
| 20230520 | RULE02 |     1.90 |
| 20230615 | RULE03 |     2.20 |

Is there anything else you'd like to do?
[T] Input transactions 
[I] Define interest rules
[P] Print statement
[Q] Quit
>
```

## Print Statement
Upon selecting Print statement option, application prompts user to select which account to print the statement for:

```
Please enter account and month to generate the statement <Account> <Year><Month>
(or enter blank to go back to main menu):
>
```

When user enters the account
```
AC001 202306
```

System then responds with the following account statement, which shows all the transactions and interest for that month (transaction type for interest is I):
```
Account: AC001
| Date     | Txn Id      | Type | Amount | Balance |
| 20230601 | 20230601-01 | D    | 150.00 |  250.00 |
| 20230626 | 20230626-01 | W    |  20.00 |  230.00 |
| 20230626 | 20230626-02 | W    | 100.00 |  130.00 |
| 20230630 |             | I    |   0.39 |  130.39 |
```

How to apply the interest rule:
* Interest is applied on end of day balance
```
| Period              | Num of days | EOD Balance | Rate Id | Rate | Annualized Interest      |
| 20230601 - 20230614 | 14          | 250         | RULE02  | 1.90 | 250 * 1.90% * 14 = 66.50 |
| 20230615 - 20230625 | 11          | 250         | RULE03  | 2.20 | 250 * 2.20% * 11 = 60.50 |
| 20230626 - 20230630 |  5          | 130         | RULE03  | 2.20 | 130 * 2.20% *  5 = 14.30 |
(this table is provided to help you get an idea how the calculation is done, it should not be displayed in the output)
```
* Therefore total interest is: (66.50 + 60.50 + 14.30) / 365 = 0.3871 => 0.39
* The interest is credited at the last day of the month

## Quit
When user chooses to quit, user enters:
```
q
```

System responds with:
```
Thank you for banking with AwesomeGIC Bank.
Have a nice day!
```