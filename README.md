# Simple Banking Console App

A small, console-based banking application written in C# (.NET) for educational purposes. It demonstrates basic banking functionality, secure credential handling (demo-level), and file-based persistence.

## Features

- Create account (username must be unique)
- Login (username + password, password entry is masked)
- Deposit and withdraw funds
- Check balance
- Display account details
- Transaction history (timestamped)
- Data persisted to `accounts.json`

## Prerequisites

- .NET SDK 8.0 or later installed (the project targets `net8.0`).
- PowerShell or another terminal on Windows.

Verify your .NET installation:

```powershell
dotnet --info
```

## Build & Run

Open PowerShell, change to the project directory and run the app:

```powershell
cd d:/workplace/bankingmanagement
dotnet build .\BankApp.csproj
dotnet run --project .\BankApp.csproj
```

When the program runs you'll see a simple menu to create an account, login, or exit.

## Usage (typical session)

1. Start the app.
2. Select `Create account`, enter full name, username and password (password is masked).
3. Select `Login`, provide credentials.
4. After login you can `Deposit`, `Withdraw`, `Check balance`, `Display account details`, and view `Transaction history`.
5. Choose `Logout` to return to the main menu or `Exit` to quit.

Sample output after login:

```
Logged in: johndoe (John Doe) — Balance: $100.50
1) Deposit
2) Withdraw
3) Check balance
4) Display account details
5) Transaction history
6) Logout
```

## Data & Persistence

- All accounts and transactions are stored in `accounts.json` in the project folder.
- Monetary values use `decimal` to avoid floating-point rounding errors.
- Example `accounts.json` entry (trimmed):

```json
[
	{
		"Id": "6b2f8e6b-...-d3b7",
		"Username": "johndoe",
		"PasswordHash": "A3F4...",
		"FullName": "John Doe",
		"Balance": 50.25,
		"CreatedAt": "2025-12-10T01:00:00Z",
		"Transactions": [ { "Date": "2025-12-10T01:05:00Z", "Type": 0, "Amount": 100.50, "Note": "Initial deposit" } ]
	}
]
```

> Note: `Type` is an enum where `0 = Deposit` and `1 = Withdrawal`.

## Security Notes

- Passwords are hashed using SHA-256 before being stored in `accounts.json` (demo-level security).
- For production use, replace SHA-256 with a slow, salted algorithm such as PBKDF2, bcrypt, or Argon2, and consider encrypting the data file or using a secure database.

## Testing

Manual test cases to validate functionality:

- Create account (unique and duplicate username)
- Login success and failure
- Deposit valid/invalid amounts
- Withdraw with sufficient and insufficient funds
- Transaction history reflects operations
- Persistence across application restarts

Automated unit tests can be added targeting `BankService` methods (`CreateAccount`, `Authenticate`, `Deposit`, `Withdraw`, `Load`, `Save`).

## Project Structure

- `Program.cs` — Console UI and main loop
- `Services/BankService.cs` — Business logic and persistence
- `Models/Account.cs` — Account model
- `Models/Transaction.cs` — Transaction model and enum
- `accounts.json` — Persistent data file

## Suggested Improvements

- Use PBKDF2/Argon2 for password hashing and add salts
- Encrypt `accounts.json` or use a database (SQLite, PostgreSQL)
- Add inter-account transfer feature
- Add role-based users (admins) and reporting
- Add unit tests and CI pipeline
- Improve CLI with `Spectre.Console` for richer UI

## License & Author

This project is an educational demo. Feel free to modify and extend it for coursework. Add your name and institution here as the author.

## Complete Source Code

### Program.cs

Main console UI and application flow (state machine).

```csharp
using System;
using BankingApp.Models;
using BankingApp.Services;

namespace BankingApp;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var service = new BankService("accounts.json");
        Account? current = null;

        while (true)
        {
            if (current == null)
            {
                Console.WriteLine();
                Console.WriteLine("=== DHAKA BANk ====");
                Console.WriteLine("1) Create account");
                Console.WriteLine("2) Login");
                Console.WriteLine("3) Exit");
                Console.Write("Choose: ");
                var k = Console.ReadLine()?.Trim();
                if (k == "1") CreateAccount(service);
                else if (k == "2") current = Login(service);
                else if (k == "3") break;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine($"Logged in: {current.Username} ({current.FullName}) — Balance: {current.Balance:C}");
                Console.WriteLine("1) Deposit");
                Console.WriteLine("2) Withdraw");
                Console.WriteLine("3) Check balance");
                Console.WriteLine("4) Display account details");
                Console.WriteLine("5) Transaction history");
                Console.WriteLine("6) Logout");
                Console.Write("Choose: ");
                var k = Console.ReadLine()?.Trim();
                if (k == "1") Deposit(service, current);
                else if (k == "2") Withdraw(service, current);
                else if (k == "3") CheckBalance(current);
                else if (k == "4") DisplayDetails(current);
                else if (k == "5") ShowHistory(current);
                else if (k == "6") current = null;
            }
        }

        Console.WriteLine("Goodbye!");
    }

    static void CreateAccount(BankService svc)
    {
        Console.Write("Full name: ");
        var full = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.Write("Username: ");
        var user = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.Write("Password: ");
        var pass = ReadPassword();

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            Console.WriteLine("Username and password cannot be empty.");
            return;
        }

        var acc = svc.CreateAccount(user, pass, full);
        if (acc == null)
            Console.WriteLine("Username already exists.");
        else
            Console.WriteLine($"Account created. Welcome, {acc.FullName}!");
    }

    static Account? Login(BankService svc)
    {
        Console.Write("Username: ");
        var user = Console.ReadLine()?.Trim() ?? string.Empty;
        Console.Write("Password: ");
        var pass = ReadPassword();

        var acc = svc.Authenticate(user, pass);
        if (acc == null) Console.WriteLine("Invalid credentials.");
        return acc;
    }

    static void Deposit(BankService svc, Account acc)
    {
        Console.Write("Amount to deposit: ");
        if (!decimal.TryParse(Console.ReadLine(), out var amt)) { Console.WriteLine("Invalid amount."); return; }
        Console.Write("Note (optional): ");
        var note = Console.ReadLine();
        var ok = svc.Deposit(acc, amt, note);
        Console.WriteLine(ok ? "Deposit successful." : "Deposit failed.");
    }

    static void Withdraw(BankService svc, Account acc)
    {
        Console.Write("Amount to withdraw: ");
        if (!decimal.TryParse(Console.ReadLine(), out var amt)) { Console.WriteLine("Invalid amount."); return; }
        Console.Write("Note (optional): ");
        var note = Console.ReadLine();
        var ok = svc.Withdraw(acc, amt, note);
        Console.WriteLine(ok ? "Withdrawal successful." : "Insufficient funds or invalid amount.");
    }

    static void CheckBalance(Account acc)
    {
        Console.WriteLine($"Current balance: {acc.Balance:C}");
    }

    static void DisplayDetails(Account acc)
    {
        Console.WriteLine("--- Account Details ---");
        Console.WriteLine($"ID: {acc.Id}");
        Console.WriteLine($"Username: {acc.Username}");
        Console.WriteLine($"Full name: {acc.FullName}");
        Console.WriteLine($"Created: {acc.CreatedAt:u}");
        Console.WriteLine($"Balance: {acc.Balance:C}");
    }

    static void ShowHistory(Account acc)
    {
        Console.WriteLine("--- Transactions ---");
        foreach (var t in acc.Transactions)
        {
            Console.WriteLine($"{t.Date:u} | {t.Type} | {t.Amount:C} | {t.Note}");
        }
    }

    static string ReadPassword()
    {
        var pass = string.Empty;
        ConsoleKeyInfo key;
        while (true)
        {
            key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
            {
                pass = pass[..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return pass;
    }
}
```

### Services/BankService.cs

Business logic layer: account management, authentication, transactions, and data persistence.

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BankingApp.Models;

namespace BankingApp.Services;

public class BankService
{
    private readonly string _dataFile;
    private readonly List<Account> _accounts = new();
    private readonly object _lock = new();

    public BankService(string dataFile = "accounts.json")
    {
        _dataFile = dataFile;
        Load();
    }

    public IEnumerable<Account> Accounts => _accounts;

    public Account? CreateAccount(string username, string password, string fullName)
    {
        lock (_lock)
        {
            if (_accounts.Any(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                return null;

            var acc = new Account
            {
                Username = username,
                PasswordHash = Hash(password),
                FullName = fullName,
                Balance = 0m,
                CreatedAt = DateTime.UtcNow
            };

            _accounts.Add(acc);
            Save();
            return acc;
        }
    }

    public Account? Authenticate(string username, string password)
    {
        var hash = Hash(password);
        lock (_lock)
        {
            return _accounts.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                && a.PasswordHash == hash);
        }
    }

    public bool Deposit(Account account, decimal amount, string? note = null)
    {
        if (amount <= 0) return false;
        lock (_lock)
        {
            account.Balance += amount;
            account.Transactions.Add(new Transaction { Date = DateTime.UtcNow, Amount = amount, Type = TransactionType.Deposit, Note = note });
            Save();
            return true;
        }
    }

    public bool Withdraw(Account account, decimal amount, string? note = null)
    {
        if (amount <= 0) return false;
        lock (_lock)
        {
            if (account.Balance < amount) return false;
            account.Balance -= amount;
            account.Transactions.Add(new Transaction { Date = DateTime.UtcNow, Amount = amount, Type = TransactionType.Withdrawal, Note = note });
            Save();
            return true;
        }
    }

    public Account? GetByUsername(string username)
    {
        lock (_lock)
        {
            return _accounts.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_dataFile)) return;
            var json = File.ReadAllText(_dataFile, Encoding.UTF8);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<Account>>(json, opts);
            if (list != null)
            {
                _accounts.Clear();
                _accounts.AddRange(list);
            }
        }
        catch
        {
            // ignore read errors for now
        }
    }

    private void Save()
    {
        try
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_accounts, opts);
            File.WriteAllText(_dataFile, json, Encoding.UTF8);
        }
        catch
        {
            // ignore write errors for now
        }
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
```

### Models/Account.cs

Data model representing a bank account.

```csharp
using System;
using System.Collections.Generic;

namespace BankingApp.Models;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
}
```

### Models/Transaction.cs

Data model representing a financial transaction.

```csharp
using System;

namespace BankingApp.Models;

public enum TransactionType
{
    Deposit,
    Withdrawal
}

public class Transaction
{
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
```

### BankApp.csproj

Project file configuration.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```
