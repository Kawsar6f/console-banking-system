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
