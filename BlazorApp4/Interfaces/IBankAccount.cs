/// <summary>
///Defines the contract for a bank account.
/// Provides identification and financial details, including balance, currency, and interest rate.
/// Includes methods to perform common banking operations such as withdraw, deposit, and transfer,
/// and exposes a transaction history.
/// </summary>
public interface IBankAccount
{
    Guid Id { get; }
    string Name { get; }
    AccountType AccountType { get; }
    Currency Currency { get; }
    decimal Balance { get; }
    DateTime LastUpdated { get; }
    decimal? InterestRate { get; }

    void Withdraw(decimal amount);
    void Deposit(decimal amount);
    void TransferTo(BankAccount toAccount, decimal amount);

    List<Transaction> Transactions { get; }
}