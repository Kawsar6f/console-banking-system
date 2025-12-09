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
