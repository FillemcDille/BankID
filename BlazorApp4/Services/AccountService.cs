

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
            await IsInitialized();
            var account = new BankAccount(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            await SaveAsync();
            return account;
        }

        public async Task<List<IBankAccount>> GetAccounts()
        {
            await IsInitialized();
            return _accounts.Cast<IBankAccount>().ToList();
        }
    }
}