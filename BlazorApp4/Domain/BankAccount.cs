using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
namespace BlazorApp4.Domain
{
    /// <summary>
    /// Represents a bank account with support for deposits, withdrawals, transfers, and interest application.
    /// </summary>
    /// <remarks>A BankAccount maintains its balance, transaction history, and account metadata such as type
    /// and currency. For savings accounts, interest can be applied using the stored interest rate. All monetary
    /// operations update the account's balance and transaction history. Thread safety is not guaranteed; concurrent
    /// access should be managed externally if required.</remarks>
    public class BankAccount : IBankAccount
    {
        // Properties
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public AccountType AccountType { get; private set; }
        public Currency Currency { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public decimal? InterestRate { get; private set; }
        public List<Transaction> Transactions => _transactions;

        private readonly List<Transaction> _transactions = new();

        // Constructors
        public BankAccount(string name, AccountType accountType, Currency currency, decimal initialBalance, decimal? interestRate = null)
        {
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = initialBalance;
            InterestRate = accountType == AccountType.Savings ? interestRate : null;
            LastUpdated = DateTime.UtcNow;
        }

        [JsonConstructor]
        public BankAccount(Guid id, string name, AccountType accountType, Currency currency, decimal balance, DateTime lastUpdated, List<Transaction>? transactions = null, decimal? interestRate = null)
        {
            Id = id;
            Name = name;
            AccountType = accountType;
            Currency = currency;
            Balance = balance;
            LastUpdated = lastUpdated;
            InterestRate = accountType == AccountType.Savings ? interestRate : null;
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

        /// <summary>
        /// Applies interest using stored InterestRate for savings accounts.
        /// </summary>
        /// <returns>The credited interest amount.</returns>
        public decimal ApplyInterest()
        {
            if (AccountType != AccountType.Savings || !(InterestRate is > 0m) || Balance <= 0m)
                return 0m;

            var interest = Math.Round(Balance * InterestRate!.Value, 2, MidpointRounding.AwayFromZero);
            if (interest == 0m) return 0m;

            Balance += interest;
            LastUpdated = DateTime.UtcNow;

            _transactions.Add(new Transaction
            {
                TransactionType = TransactionType.Interest,
                Amount = interest,
                BalanceAfter = Balance,
                ToAccountId = Id,
                TimeStamp = DateTime.UtcNow
            });

            return interest;
        }
    }
}
