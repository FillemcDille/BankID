namespace BlazorApp4.Services
{
    public class AccountService : IAccountService
    {
        private const string StorageKey = "bankapp.accounts";
        private readonly List<IBankAccount> _accounts;
        private readonly IStorageService _storageService;
        private readonly ILogger<AccountService> _logger;
        private bool isLoaded;

        public AccountService(IStorageService storageService, ILogger<AccountService> logger)
        {
            _storageService = storageService;
            _logger = logger;
            _accounts = new List<IBankAccount>();
        }

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

        private Task SaveAsync()
        {
            _logger.LogInformation("Saving {Count} accounts to storage.", _accounts.Count);
            return _storageService.SetItemAsync(StorageKey, _accounts);
        }

        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance)
        {
            await IsInitialized();
            var account = new BankAccount(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            _logger.LogInformation("Created account {Name} ({Id}) with balance {Balance} {Currency}.", name, account.Id, initialBalance, currency);
            await SaveAsync();
            return account;
        }

        public List<IBankAccount> GetAccounts() => _accounts.Cast<IBankAccount>().ToList();

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

        public async Task EnsureLoadedAsync()
        {
            if (isLoaded) return;
            await IsInitialized();
            isLoaded = true;
        }

        public async Task WidrawAsync(Guid accountId, decimal amount)
        {
            await IsInitialized();

            var account = _accounts.OfType<BankAccount>().FirstOrDefault(a => a.Id == accountId)
                ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");

            account.Withdraw(amount);
            _logger.LogInformation("Withdraw {Amount} from {Account}. New balance {Balance}.", amount, accountId, account.Balance);
            await SaveAsync();
        }

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
