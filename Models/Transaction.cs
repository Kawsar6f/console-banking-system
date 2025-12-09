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
