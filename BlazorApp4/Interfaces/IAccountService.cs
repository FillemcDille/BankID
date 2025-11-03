namespace BlazorApp4.Interfaces
{  
    /// <summary>
    /// Defines operations for managing bank accounts, including account creation, retrieval, fund transfers, and
    /// ensuring account data is loaded.
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
        
       
    }
}
