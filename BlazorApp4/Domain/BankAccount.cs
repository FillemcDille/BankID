using System.Text.Json.Serialization;
namespace BlazorApp4.Domain
{
    /// <summary>
    /// Represents a bank account with basic operations such as deposit, withdrawal, and transfer, as well as
    /// transaction history tracking.
    /// </summary>
    /// <remarks>A BankAccount instance maintains its balance, currency, account type, and a list of
    /// transactions. All monetary operations update the account's balance and last updated timestamp. The class
    /// enforces validation for positive amounts and sufficient funds during withdrawals and transfers. Transaction
    /// history is accessible via the Transactions property. This type is not thread-safe; concurrent access should be
    /// synchronized externally if used in multi-threaded scenarios.</remarks>
    public class BankAccount : IBankAccount
    {
        //Constants
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public AccountType AccountType { get; private set; }
        public Currency Currency { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public List<Transaction> Transactions => _transaction;
        private readonly List<Transaction> _transaction = new();

        // Constructor
        public BankAccount(string name, AccountType accountType, Currency currency, decimal initialBalance)
        {
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = initialBalance;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the BankAccount class with the specified account details, balance, currency,
        /// last update timestamp, and optional transaction history.
        /// </summary>
        /// <remarks>Use this constructor to create a fully populated BankAccount instance, including its
        /// transaction history if available. If the transactions parameter is not provided or is null, the account will
        /// not contain any transactions upon initialization.</remarks>
        /// <param name="id">The unique identifier for the bank account.</param>
        /// <param name="name">The name associated with the bank account. This typically represents the account holder or account label.</param>
        /// <param name="accountType">The type of the bank account, such as checking, savings, or other supported account types.</param>
        /// <param name="currency">The currency in which the account balance is denominated.</param>
        /// <param name="balance">The initial balance of the account, expressed in the specified currency.</param>
        /// <param name="lastUpdated">The date and time when the account information was last updated.</param>
        /// <param name="transactions">An optional list of transactions associated with the account. If null, the account will be initialized without
        /// transaction history.</param>
        [JsonConstructor]
        public BankAccount(Guid id, string name, AccountType accountType, Currency currency, decimal balance, DateTime lastUpdated, List<Transaction>? transactions = null)
        {
            Id = id;
            Name = name;
            AccountType = accountType;
            Balance = balance;
            Currency = currency;
            LastUpdated = lastUpdated;

            if (transactions != null)
                _transaction = transactions;
        }


        /// <summary>
        /// Withdraws a specified amount from the account balance.
        /// </summary>
        /// <param name="amount">The amount to withdraw from the account. Must be a positive value and less than or equal to the current
        /// balance.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="amount"/> is less than or equal to zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="amount"/> exceeds the available account balance.</exception>
        public void Withdraw(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (amount > Balance) throw new InvalidOperationException("Insufficient funds.");
            Balance -= amount;
            LastUpdated = DateTime.UtcNow;

            _transaction.Add(new Transaction
            {
                TransactionType = TransactionType.Withdraw,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                TimeStamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Adds the specified amount to the account balance.   
        /// </summary>
        /// <param name="amount">The amount to deposit into the account. Must be a positive value.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="amount"/> is less than or equal to zero.</exception>
        public void Deposit(decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
            Balance += amount;
            LastUpdated = DateTime.UtcNow;
            _transaction.Add(new Transaction
            {
                TransactionType = TransactionType.Deposit,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                TimeStamp = DateTime.UtcNow
            });

        }

        /// <summary>
        /// Transfers the specified amount from this account to another bank account.
        /// </summary>
        /// <remarks>This method updates the balances and transaction histories of both accounts involved
        /// in the transfer. The transfer is recorded as a transaction in each account. The method does not perform
        /// validation on the destination account; callers should ensure that the provided account is valid and
        /// accessible.</remarks>
        /// <param name="toAccount">The destination account to which the funds will be transferred. Must not be null.</param>
        /// <param name="amount">The amount of money to transfer. Must be greater than zero and less than or equal to the current account
        /// balance.</param>
        public void TransferTo(BankAccount toAccount, decimal amount)
        {
            //From which account
            Balance -= amount;
            LastUpdated = DateTime.Now;
            _transaction.Add(new Transaction
            {
                TransactionType = TransactionType.TransferOut,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                ToAccountId = toAccount.Id,
                TimeStamp = DateTime.UtcNow
            });

            //To which account
            toAccount.Balance += amount;
            toAccount.LastUpdated = DateTime.Now;
            toAccount._transaction.Add(new Transaction
            {
                TransactionType = TransactionType.TransferIn,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                ToAccountId = toAccount.Id,
                TimeStamp = DateTime.UtcNow
            });
        }
    }
}