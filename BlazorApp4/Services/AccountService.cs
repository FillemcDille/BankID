

namespace BlazorApp4.Services
{
    public class AccountService : IAccountService
    {
        private const string StorageKey = "bankapp.accounts";
        private readonly List<IBankAccount> _accounts;
        private readonly IStorageService _storageService;
        private bool isLoaded;
        public List<Transaction> Transactions { get; set; } = new();

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
        public async Task<List<Transaction>> GetTransactionsAsync(string accountName)
        {
            var allTransactions = await _storageService.GetItemAsync<List<Transaction>>("Transactions") ?? new();
            return allTransactions
                .Where(t => t.AccountName.Equals(accountName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Date)
                .ToList();
        }
        public async Task<List<Transaction>> GetTransactionsHistoryAsync()
        {
            return await _storageService.GetItemAsync<List<Transaction>>("Transactions") ?? new();
        }

        private async Task LogTransaction(Transaction tx)
        {
            var transaction = await _storageService.GetItemAsync<List<Transaction>>("Transactions") ?? new();
            transaction.Add(tx);
            await _storageService.SetItemAsync("Transactions", transaction);
        }
        public async Task DepositAsync(Guid accountId, decimal amount)
        {
            await IsInitialized();
            var account = _accounts.FirstOrDefault(a => a.Id == accountId);
            if (account is null) throw new Exception("Konto hittades inte.");

            account.Deposit(amount);
            await SaveAsync();

            await LogTransaction(new Transaction
            {
                AccountName = account.Name,
                Amount = amount,
                BalanceAfter = account.Balance,
                Description = "Insättning",
                Type = TransactionType.Insättning,
                Date = DateTime.Now
            });
        }
    }

}