/// <summary>
/// Defines the contract for a bank account, including properties for account identification, type, currency, balance,
/// and transaction history, as well as methods for withdrawing, depositing, and transferring funds.
/// </summary>
/// <remarks>Implementations of this interface should ensure that account operations such as withdrawals,
/// deposits, and transfers update the balance and transaction history appropriately. Thread safety and validation of
/// input parameters (such as ensuring sufficient funds for withdrawals and transfers) are recommended for robust
/// implementations.</remarks>
public interface IBankAccount
{
    Guid Id { get; }
    string Name { get; }
    AccountType AccountType { get; }
    Currency Currency { get; }
    decimal Balance { get; }
    DateTime LastUpdated { get; }

    void Withdraw(decimal amount);
    void Deposit(decimal amount);
    void TransferTo(BankAccount toAccount, decimal amount);

    List<Transaction> Transactions { get; }
}