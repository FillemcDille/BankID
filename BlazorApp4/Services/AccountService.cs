using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorApp4.Services
{
    /// <summary>
    /// Provides services for managing bank accounts, including creation, retrieval, transactions, interest application,
    /// and import/export operations. Supports asynchronous persistence and event notification for account state
    /// changes.
    /// </summary>
    /// <remarks>AccountService coordinates account data storage, logging, and business operations such as
    /// transfers, deposits, withdrawals, and interest accrual. It ensures that account data is loaded from persistent
    /// storage before operations are performed and provides methods for importing and exporting account data in JSON
    /// format. The service raises events to notify listeners of state changes and supports background interest
    /// application. This class is not thread-safe; concurrent access should be externally synchronized if used in
    /// multi-threaded scenarios.</remarks>
    public class AccountService : IAccountService, IDisposable
    {

        private const string StorageKey = "bankapp.accounts";
        private readonly List<BankAccount> _accounts;
        private readonly IStorageService _storageService;
        private readonly ILogger<AccountService> _logger;
        private bool isLoaded;
        private bool isRunning;
        
        public event Action? action;
        public event Action? StatehasChanged;

        public void NotifyEvent() => action?.Invoke();

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
            _accounts = new List<BankAccount>();
        }

        /// <summary>
        /// Initializes the account collection by loading data from persistent storage if it has not already been
        /// loaded.
        /// </summary>
        /// <remarks>This method should be called before performing operations that require the account
        /// data to be loaded. If the data is already loaded, the method completes immediately. Interest is
        /// automatically applied to eligible savings accounts based on the elapsed time since the last update, and
        /// changes are saved if any interest is credited.</remarks>
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
            NotifyEvent();
        }

        /// <summary>
        /// Applies accrued interest to all eligible savings accounts and persists changes if any interest is credited.
        /// </summary>
        /// <remarks>Interest is applied only to accounts of type Savings with a positive interest rate.
        /// Changes are saved only if at least one account receives credited interest.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task AppliedInterest()
        {
            var anyApplied = false;

            foreach (var account in _accounts.OfType<BankAccount>())
            {
                if (account.AccountType != AccountType.Savings || account.InterestRate is not > 0m)
                    continue;

                var credited = account.ApplyInterest();
                if (credited > 0m)
                    anyApplied = true;
            }

            if (anyApplied)
                await SaveAsync();
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
        public async Task<BankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance, decimal? interestRate = null)
        {
            await IsInitialized();
            var account = new BankAccount(name, accountType, currency, initialBalance, interestRate);
            _accounts.Add(account);
            _logger.LogInformation("Created account {Name} ({Id}) with balance {Balance} {Currency}. Rate={Rate}", name, account.Id, initialBalance, currency, interestRate);
            await SaveAsync();
            return account;
        }

        /// <summary>
        /// Retrieves a list of all bank accounts managed by this instance. 
        /// </summary>
        /// <returns>A list of objects implementing <see cref="BankAccount"/> representing all accounts. The list will be empty
        /// if no accounts are available.</returns>
        public IReadOnlyList<BankAccount> GetAccounts() => _accounts;

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
            _logger.LogInformation("Transfer {Amount} from {From} to {To}.", amount, fromAccountId, toAccountId);

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
        public async Task WithdrawAsync(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            account.Withdraw(amount);
            _logger.LogInformation("Withdraw {Amount} from {Account}.", amount, accountId);
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
            _logger.LogInformation("Deposit {Amount} to {Account}.", amount, accountId);
            await SaveAsync();
        }

        /// <summary>
        /// Applies interest to the specified bank account asynchronously.
        /// </summary>
        /// <remarks>The interest is calculated based on the account's balance and interest rate, and the
        /// resulting amount is credited to the account. The operation is logged and persisted. Ensure that the account
        /// exists and is loaded before calling this method.</remarks>
        /// <param name="accountId">The unique identifier of the account to which interest will be applied.</param>
        /// <returns>A task that represents the asynchronous interest application operation. The task result contains the
        /// amount of money credited as interest.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no account with the specified account ID exists.</exception>
        public async Task<decimal> ApplyInterestAsync(Guid accountId)
        {
            await IsInitialized();
            var account = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");
            var credited = account.ApplyInterest();
            _logger.LogInformation("Applied interest to {Account}. Credited={Credited}", accountId, credited);
            await SaveAsync();
            return credited;
            
        }

        /// <summary>
        /// Provides default options for JSON serialization, including indented formatting, case-insensitive property
        /// names, and camel case enum string conversion.
        /// </summary>
        /// <remarks>These options ensure that serialized JSON output is human-readable and compatible
        /// with clients expecting camel case enum values and case-insensitive property matching. The settings are
        /// suitable for most scenarios where consistent and readable JSON is required.</remarks>
        private static readonly JsonSerializerOptions _jsonOption = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
       
        /// <summary>
        /// Asynchronously exports the current account data to a JSON-formatted string.
        /// </summary>
        /// <remarks>The returned JSON string reflects the current state of the account collection. Ensure
        /// that the data is loaded before calling this method to obtain up-to-date results.</remarks>
        /// <returns>A string containing the serialized account data in JSON format.</returns>
        public async Task<string> ExportJsonAsync()
        {
            await EnsureLoadedAsync();
            return JsonSerializer.Serialize(_accounts, _jsonOption);
        }

        /// <summary>
        /// Imports a collection of bank accounts from a JSON string asynchronously, optionally replacing existing
        /// accounts.
        /// </summary>
        /// <remarks>If the JSON is invalid, empty, or contains no data, the returned list will include
        /// corresponding error messages. When adding accounts without replacing, only accounts with unique identifiers
        /// not already present are imported.</remarks>
        /// <param name="json">A JSON-formatted string representing a list of bank accounts to import. Cannot be null or whitespace.</param>
        /// <param name="replaceExisting">If <see langword="true"/>, replaces all existing accounts with the imported accounts; otherwise, adds only
        /// accounts that do not already exist.</param>
        /// <returns>A list of error messages encountered during the import process. The list will be empty if the import
        /// succeeds without errors.</returns>
        public async Task<List<string>?> ImportJsonAsync(string json, bool replaceExisting = false)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(json))
            {
                errors.Add("Empty Json");
                return errors;
            }

            List<BankAccount>? incoming = null;
            try
            {
                incoming = JsonSerializer.Deserialize<List<BankAccount>>(json, _jsonOption);
            }
            catch
            {
                errors.Add("Invalid, JSON");
            }

            if (incoming is null || incoming.Count == 0)
            {
                errors.Add("No Data");
                return errors;
            }
            await EnsureLoadedAsync();

            if (replaceExisting)
            {
                _accounts.Clear();
                _accounts.AddRange(incoming);
            }
            else
            {
                var existing = _accounts.Select(a => a.Id).ToHashSet();
                foreach (var a in incoming)
                    if (!existing.Contains(a.Id))
                        _accounts.Add(a);
            }

            await SaveAsync();
            return errors;
        }

        public void Dispose()
        {
            
        }
        
        /// <summary>
        /// Starts a background process that periodically applies interest and notifies listeners of state changes.
        /// </summary>
        /// <remarks>This method initiates an asynchronous loop that applies interest at regular
        /// intervals. The process continues to run until explicitly stopped. This method is not thread-safe; ensure
        /// that it is not called concurrently from multiple threads.</remarks>
        public void AutoApplyInterest()
        {
            isRunning = true;
            
            Task.Run(async () => { 
            
                while(isRunning)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await AppliedInterest();
                    StatehasChanged?.Invoke();
                }
            });
        }
    }
}
