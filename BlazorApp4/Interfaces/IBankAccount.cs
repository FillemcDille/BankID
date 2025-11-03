//Defines the contract for a bank account, including properties for identification, account details, balance, and
//transaction history, as well as methods for performing withdrawals, deposits, and transfers.
public interface IBankAccount
{
    Guid Id { get; }
    string Name { get; }
    AccountType AccountType { get; }
    Currency Currency { get; }
    decimal Balance { get; }
    DateTime LastUpdated { get; }
    decimal? InterestRate { get; } // fraction, e.g. 0.01m == 1%

    void Withdraw(decimal amount);
    void Deposit(decimal amount);
    void TransferTo(BankAccount toAccount, decimal amount);

    List<Transaction> Transactions { get; }
}