using System.Text.Json.Serialization;
namespace BlazorApp4.Domain
{
    /// <summary>
    /// Represents a bank account that supports deposits, withdrawals, transfers, and transaction history.
    /// </summary>
    public class BankAccount : IBankAccount
    {
        // Properties
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public AccountType AccountType { get; private set; }
        public Currency Currency { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public List<Transaction> Transactions => _transactions;

        private readonly List<Transaction> _transactions = new();

        // Constructors
        public BankAccount(string name, AccountType accountType, Currency currency, decimal initialBalance)
        {
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = initialBalance;
            LastUpdated = DateTime.UtcNow;
        }

        [JsonConstructor]
        public BankAccount(Guid id, string name, AccountType accountType, Currency currency, decimal balance, DateTime lastUpdated, List<Transaction>? transactions = null)
        {
            Id = id;
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = balance;
            LastUpdated = lastUpdated;
            if (transactions != null)
                _transactions = transactions;
        }

        /// <summary>
        /// Withdraws the specified amount from the account.
        /// </summary>
        /// <exception cref="ArgumentException">If the amount is not positive.</exception>
        /// <exception cref="InvalidOperationException">If balance is insufficient.</exception>
        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (amount > Balance)
                throw new InvalidOperationException("Insufficient funds.");

            Balance -= amount;
            LastUpdated = DateTime.UtcNow;

            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.Withdraw,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                TimeStamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Deposits the specified amount into the account.
        /// </summary>
        /// <exception cref="ArgumentException">If the amount is not positive.</exception>
        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));

            Balance += amount;
            LastUpdated = DateTime.UtcNow;

            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.Deposit,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                TimeStamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Transfers the specified amount to another account.
        /// </summary>
        /// <param name="toAccount">The target account.</param>
        /// <param name="amount">The amount to transfer.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="toAccount"/> is null.</exception>
        /// <exception cref="ArgumentException">If amount is not positive.</exception>
        /// <exception cref="InvalidOperationException">If balance is insufficient.</exception>
        public void TransferTo(BankAccount toAccount, decimal amount)
        {
            if (toAccount == null)
                throw new ArgumentNullException(nameof(toAccount));
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (amount > Balance)
                throw new InvalidOperationException("Insufficient funds.");

            // From account
            Balance -= amount;
            LastUpdated = DateTime.UtcNow;

            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.TransferOut,
                Amount = amount,
                BalanceAfter = Balance,
                FromAccountId = Id,
                ToAccountId = toAccount.Id,
                TimeStamp = DateTime.UtcNow
            });

            // To account
            toAccount.Balance += amount;
            toAccount.LastUpdated = DateTime.UtcNow;

            toAccount._transactions.Add(new Transaction
            {
                TransactionType = TransactionType.TransferIn,
                Amount = amount,
                BalanceAfter = toAccount.Balance,
                FromAccountId = Id,
                ToAccountId = toAccount.Id,
                TimeStamp = DateTime.UtcNow
            });
        }
    }
}
