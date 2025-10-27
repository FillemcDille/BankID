


namespace BlazorApp4.Services
{
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
        /// Ensures that the account data is loaded from storage if it has not already been initialized.    
        /// </summary>
        /// <remarks>Subsequent calls have no effect if the data is already loaded. This method should be
        /// awaited before accessing account data to ensure it is available.</remarks>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
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
        /// Asynchronously saves the current account data to persistent storage.
        /// </summary>
        /// <remarks>This method does not block the calling thread. Await the returned task to ensure the
        /// save operation completes before proceeding. If the underlying storage service encounters an error, the
        /// returned task may fault with an exception.</remarks>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        private Task SaveAsync() => _storageService.SetItemAsync(StorageKey, _accounts);

        /// <summary>
        /// Creates a new bank account with the specified name, account type, currency, and initial balance.    
        /// </summary>
        /// <remarks>The new account is added to the internal account collection and persisted
        /// asynchronously. If the initial balance is negative, an exception may be thrown by the account
        /// constructor.</remarks>
        /// <param name="name">The name to assign to the new bank account. Cannot be null or empty.</param>
        /// <param name="accountType">The type of account to create, such as checking or savings.</param>
        /// <param name="currency">The currency in which the account will operate.</param>
        /// <param name="initialBalance">The initial balance to set for the account. Must be greater than or equal to zero.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created bank account.</returns>
        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance)
        {
            var account = new BankAccount(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            await SaveAsync();
            return account;
        }

        /// <summary>
        /// Retrieves a list of all bank accounts managed by this instance.
        /// </summary>
        /// <returns>A list of objects implementing <see cref="IBankAccount"/> representing the current bank accounts. The list
        /// will be empty if no accounts are available.</returns>
        public List<IBankAccount> GetAccounts()
        {
            return _accounts.Cast<IBankAccount>().ToList();
        }

        /// <summary>
        /// Transfers the specified amount from one bank account to another asynchronously.
        /// </summary>
        /// <remarks>Both accounts must exist and the source account must have sufficient funds for the
        /// transfer to succeed.</remarks>
        /// <param name="fromAccountId">The unique identifier of the account from which funds will be withdrawn.</param>
        /// <param name="toAccountId">The unique identifier of the account to which funds will be deposited.</param>
        /// <param name="amount">The amount to transfer. Must be a positive value and less than or equal to the balance of the source
        /// account.</param>
        /// <returns>A task that represents the asynchronous transfer operation.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if either <paramref name="fromAccountId"/> or <paramref name="toAccountId"/> does not correspond to
        /// an existing account.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the source account does not have sufficient funds to complete the transfer.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="amount"/> is less than or equal to zero.</exception>
        public async Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var fromAccount = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == fromAccountId)
            ?? throw new KeyNotFoundException($"Account with ID {fromAccountId} not found.");
            
            var toAccount =_accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == toAccountId)
            ?? throw new KeyNotFoundException($"Account with ID {toAccountId} not found.");

            if (fromAccount.Balance < amount)
                throw new InvalidOperationException("Otillräckliga medel på från-kontot.");
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Beloppet måste vara positivt.");

            fromAccount .TransferTo(toAccount, amount);
            await SaveAsync();
        }

        /// <summary>
        /// Ensures that the object is loaded asynchronously, performing initialization if it has not already been
        /// completed.
        /// </summary>
        /// <remarks>If the object is already loaded, this method returns immediately without performing
        /// any additional actions. This method is safe to call multiple times; initialization will only occur
        /// once.</remarks>
        /// <returns>A task that represents the asynchronous load operation. The task completes when loading and initialization
        /// are finished.</returns>
        public async Task EnsureLoadedAsync()
        {
            if (isLoaded)
                return;
            await IsInitialized(); 

            isLoaded = true;
        }
    }
}