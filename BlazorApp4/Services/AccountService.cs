namespace BlazorApp4.Services
{
    /// <summary>
    /// Provides operations for managing bank accounts, including creation, retrieval, deposit, withdrawal, and transfer
    /// of funds. Supports asynchronous persistence and retrieval of account data from storage.
    /// </summary>
    /// <remarks>This service coordinates account-related actions and ensures that all changes are logged and
    /// persisted using the underlying storage and logging services. All account operations are performed asynchronously
    /// to avoid blocking the calling thread. Before performing account operations, ensure that the service is loaded by
    /// calling <see cref="EnsureLoadedAsync"/> if necessary. The service is not thread-safe; concurrent access should
    /// be managed externally if required.</remarks>
    public class AccountService : IAccountService
    {
        private const string StorageKey = "bankapp.accounts";
        private readonly List<IBankAccount> _accounts;
        private readonly IStorageService _storageService;
        private readonly ILogger<AccountService> _logger;
        private bool isLoaded;

        /// <summary>
        /// Initializes a new instance of the AccountService class with the specified storage service and logger.
        /// </summary>
        /// <param name="storageService">The storage service used to persist and retrieve account data. Cannot be null.</param>
        /// <param name="logger">The logger used for recording operational and error information related to account operations. Cannot be
        /// null.</param>
        public AccountService(IStorageService storageService, ILogger<AccountService> logger)
        {
            _storageService = storageService;
            _logger = logger;
            _accounts = new List<IBankAccount>();
        }

        /// <summary>
        /// Ensures that the account data is loaded from persistent storage if it has not already been initialized.
        /// </summary>
        /// <remarks>Subsequent calls have no effect if the data has already been loaded. This method
        /// should be called before accessing account data to ensure it is available.</remarks>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        private async Task IsInitialized()
        {
            if (isLoaded) return;

            var fromStorage = await _storageService.GetItemAsync<List<BankAccount>>(StorageKey);
            _accounts.Clear();

            if (fromStorage is { Count: > 0 })
            {
                _accounts.AddRange(fromStorage);
                _logger.LogInformation("Loaded {Count} accounts from storage.", fromStorage.Count);
            }
            else
            {
                _logger.LogInformation("No accounts found in storage.");
            }

            isLoaded = true;
        }

        /// <summary>
        /// Asynchronously saves the current collection of accounts to persistent storage.
        /// </summary>
        /// <remarks>The save operation writes all accounts in the collection to the underlying storage
        /// service. This method does not block the calling thread. If the storage service fails, the returned task will
        /// be faulted with the corresponding exception.</remarks>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        private Task SaveAsync()
        {
            _logger.LogInformation("Saving {Count} accounts to storage.", _accounts.Count);
            return _storageService.SetItemAsync(StorageKey, _accounts);
        }

        /// <summary>
        /// Creates a new bank account with the specified name, account type, currency, and initial balance.
        /// </summary>
        /// <remarks>The account is added to the internal account collection and persisted asynchronously.
        /// The method logs account creation details for auditing purposes.</remarks>
        /// <param name="name">The name to assign to the new account. Cannot be null or empty.</param>
        /// <param name="accountType">The type of account to create, such as checking or savings.</param>
        /// <param name="currency">The currency in which the account will operate.</param>
        /// <param name="initialBalance">The initial balance to set for the account. Must be a non-negative value.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created bank account.</returns>
        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance)
        {
            await IsInitialized();
            var account = new BankAccount(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            _logger.LogInformation("Created account {Name} ({Id}) with balance {Balance} {Currency}.", name, account.Id, initialBalance, currency);
            await SaveAsync();
            return account;
        }

        /// <summary>
        /// Retrieves a list of all bank accounts managed by this instance. 
        /// </summary>
        /// <returns>A list of objects implementing <see cref="IBankAccount"/> representing all accounts. The list will be empty
        /// if no accounts are available.</returns>
        public List<IBankAccount> GetAccounts() => _accounts.Cast<IBankAccount>().ToList();

        /// <summary>
        /// Transfers the specified amount from one bank account to another.
        /// </summary>
        /// <param name="fromAccountId">The unique identifier of the account from which funds will be withdrawn.</param>
        /// <param name="toAccountId">The unique identifier of the account to which funds will be deposited.</param>
        /// <param name="amount">The amount of money to transfer. Must be a positive value.</param>
        /// <returns>A task that represents the asynchronous transfer operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if either <paramref name="fromAccountId"/> or <paramref name="toAccountId"/> does not correspond to
        /// an existing account.</exception>
        public async Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            await IsInitialized();

            var fromAccount = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == fromAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {fromAccountId} not found.");

            var toAccount = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == toAccountId)
                ?? throw new KeyNotFoundException($"Account with ID {toAccountId} not found.");

            fromAccount.TransferTo(toAccount, amount);
            _logger.LogInformation("Transfer {Amount} from {From} to {To}. New balances: from={FromBal}, to={ToBal}.",
                amount, fromAccountId, toAccountId, fromAccount.Balance, toAccount.Balance);

            await SaveAsync();
        }

        /// <summary>
        /// Ensures that the object is fully loaded and initialized asynchronously. If the object is already loaded, the
        /// method completes immediately.
        /// </summary>
        /// <remarks>This method should be called before performing operations that require the object to
        /// be loaded. Subsequent calls have no effect if the object is already loaded.</remarks>
        /// <returns>A task that represents the asynchronous load operation. The task completes when the object is loaded.</returns>
        public async Task EnsureLoadedAsync()
        {
            if (isLoaded) return;
            await IsInitialized();
            isLoaded = true;
        }

        /// <summary>
        /// Withdraws the specified amount from the bank account identified by the given account ID asynchronously.
        /// </summary>
        /// <remarks>The account balance is updated upon successful withdrawal, and the operation is
        /// logged. Ensure that the account exists and has sufficient funds before calling this method.</remarks>
        /// <param name="accountId">The unique identifier of the account from which the amount will be withdrawn.</param>
        /// <param name="amount">The amount to withdraw from the account. Must be a positive value and not exceed the account's available
        /// balance.</param>
        /// <returns>A task that represents the asynchronous withdrawal operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if an account with the specified account ID does not exist.</exception>
        public async Task WidrawAsync(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            account.Withdraw(amount);
            _logger.LogInformation("Withdraw {Amount} from {Account}. New balance {Balance}.", amount, accountId, account.Balance);
            await SaveAsync();
        }

        /// <summary>
        /// Asynchronously deposits the specified amount into the bank account identified by the given account ID.
        /// </summary>
        /// <remarks>The account balance is updated immediately upon deposit. The operation is logged and
        /// persisted asynchronously. Ensure that the account exists before calling this method.</remarks>
        /// <param name="accountId">The unique identifier of the account to which the deposit will be made.</param>
        /// <param name="amount">The amount to deposit into the account. Must be a positive value.</param>
        /// <returns>A task that represents the asynchronous deposit operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no account with the specified account ID exists.</exception>
        public async Task DepositAsync(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            account.Deposit(amount);
            _logger.LogInformation("Deposit {Amount} to {Account}. New balance {Balance}.", amount, accountId, account.Balance);
            await SaveAsync();
        }
    }
}
