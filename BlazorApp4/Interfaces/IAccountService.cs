namespace BlazorApp4.Interfaces
{  
    /// <summary>
    /// Defines the contract for account-related operations, including creating accounts, managing balances, performing
    /// transfers, and importing or exporting account data.
    /// </summary>
    public interface IAccountService
    {
        Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance, decimal? interestRate = null);
        List<IBankAccount> GetAccounts();
        Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount);
        Task EnsureLoadedAsync();
        Task WithdrawAsync(Guid accountId, decimal amount);
        Task DepositAsync(Guid accountId, decimal amount);
        Task<decimal> ApplyInterestAsync(Guid accountId);
        Task<string> ExportJsonAsync();
        Task<List<string>?> ImportJsonAsync(string json, bool replace);

    }
}
