namespace BlazorApp4.Services
{
    /// <summary>
    /// Provides functionality for managing bank accounts, including creation, transfers, deposits,
    /// and withdrawals. Handles persistence of account data through an injected <see cref="IStorageService"/>.
    /// </summary>
    public class AccountService : IAccountService
    {
        private const string StorageKey = "bankapp.accounts";
        private readonly List<IBankAccount> _accounts;
        private readonly IStorageService _storageService;
        private bool isLoaded;

        public AccountService(IStorageService storageService)
        {
            _storageService = storageService;
            _accounts = new List<IBankAccount>();
        }

        /// <summary>
        /// Ensures account data is loaded from persistent storage if not already initialized.
        /// </summary>
        private async Task IsInitialized()
        {
            if (isLoaded) return;

            var fromStorage = await _storageService.GetItemAsync<List<BankAccount>>(StorageKey);
            _accounts.Clear();

            if (fromStorage is { Count: > 0 })
                _accounts.AddRange(fromStorage);

            isLoaded = true;
        }

        /// <summary>
        /// Persists all account data asynchronously to storage.
        /// </summary>
        private Task SaveAsync() => _storageService.SetItemAsync(StorageKey, _accounts);

        /// <summary>
        /// Creates a new bank account and saves it to storage.
        /// </summary>
        /// <param name="name">The name of the account.</param>
        /// <param name="accountType">The type of the account (e.g., Deposit, Savings).</param>
        /// <param name="currency">The account currency.</param>
        /// <param name="initialBalance">The opening balance; must be non-negative.</param>
        /// <returns>The newly created account.</returns>
        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance)
        {
            var account = new BankAccount(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            await SaveAsync();
            return account;
        }

        /// <summary>
        /// Retrieves all stored accounts.
        /// </summary>
        /// <returns>A list of all <see cref="IBankAccount"/> instances.</returns>
        public List<IBankAccount> GetAccounts() =>
            _accounts.Cast<IBankAccount>().ToList();

        /// <summary>
        /// Transfers money between two accounts asynchronously.
        /// </summary>
        /// <param name="fromAccountId">The source account ID.</param>
        /// <param name="toAccountId">The destination account ID.</param>
        /// <param name="amount">The transfer amount.</param>
        /// <exception cref="KeyNotFoundException">If one or both accounts are not found.</exception>
        /// <exception cref="InvalidOperationException">If insufficient funds are available.</exception>
        public async Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var fromAccount = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == fromAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {fromAccountId} not found.");

            var toAccount = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == toAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {toAccountId} not found.");

            fromAccount.TransferTo(toAccount, amount);
            await SaveAsync();
        }

        /// <summary>
        /// Ensures all account data is loaded from storage if not already available.
        /// </summary>
        public async Task EnsureLoadedAsync()
        {
            if (isLoaded)
                return;

            await IsInitialized();
            isLoaded = true;
        }

        /// <summary>
        /// Withdraws a specified amount from an account.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="amount">The withdrawal amount.</param>
        /// <exception cref="KeyNotFoundException">If the account is not found.</exception>
        public async Task WidrawAsync(Guid accountId, decimal amount)
        {
            var account = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            account.Withdraw(amount);
            await SaveAsync();
        }

        /// <summary>
        /// Deposits a specified amount into an account.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <param name="amount">The deposit amount.</param>
        /// <exception cref="KeyNotFoundException">If the account is not found.</exception>
        public async Task DepositAsync(Guid accountId, decimal amount)
        {
            var account = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            account.Deposit(amount);
            await SaveAsync();
        }
    }
}
