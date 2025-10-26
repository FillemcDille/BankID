


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

        private async Task IsInitialized()
        {
            if (isLoaded) return;
            var fromStorage = await _storageService.GetItemAsync<List<BankAccount>>(StorageKey);
            _accounts.Clear();
            if (fromStorage is { Count: > 0 })
                _accounts.AddRange(fromStorage);
            isLoaded = true;
        }

        private Task SaveAsync() => _storageService.SetItemAsync(StorageKey, _accounts);

        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance)
        {
            //await IsInitialized();
            var account = new BankAccount(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            await SaveAsync();
            return account;
        }

        public List<IBankAccount> GetAccounts()
        {
            //await IsInitialized();
            
            return _accounts.Cast<IBankAccount>().ToList();
        }

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

      

        public async Task EnsureLoadedAsync()
        {
            if (isLoaded)
                return;
            await IsInitialized(); // Async sen betyg blir mindre

            isLoaded = true;
        }

    }
}