namespace BlazorApp4.Interfaces
{
    /// <summary>
    /// Defines the contract for all bank account operations, including:
    /// creating and retrieving accounts, managing balances (deposit, withdraw, transfer),
    /// applying and auto-updating interest for savings accounts,
    /// and importing/exporting account data in JSON format.
    /// 
    /// Implementations are expected to handle persistence and notify UI components
    /// through the <see cref="StatehasChanged"/> event when data changes.
    /// </summary>
    public interface IAccountService
    {
        Task<BankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance, decimal? interestRate = null);
        IReadOnlyList<BankAccount> GetAccounts();
        Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount);
        Task EnsureLoadedAsync();
        Task WithdrawAsync(Guid accountId, decimal amount);
        Task DepositAsync(Guid accountId, decimal amount);
        Task<decimal> ApplyInterestAsync(Guid accountId);
        Task<string> ExportJsonAsync();
        Task<List<string>?> ImportJsonAsync(string json, bool replace);
        
        event Action? StatehasChanged;

        void AutoApplyInterest();

    }
}
