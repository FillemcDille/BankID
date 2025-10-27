namespace BlazorApp4.Interfaces
{  
    /// <summary>
    /// Defines operations for managing bank accounts, including account creation, retrieval, fund transfers, and
    /// ensuring account data is loaded.
    /// </summary>
    /// <remarks>Implementations of this interface provide asynchronous and synchronous methods for
    /// interacting with bank accounts. Methods may throw exceptions for invalid input or failed operations; refer to
    /// individual member documentation for details. This interface is intended to be used by services or applications
    /// that require account management functionality.</remarks>
    public interface IAccountService
    {
        Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance);
        List<IBankAccount> GetAccounts();
        Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount);
        Task EnsureLoadedAsync();
       
    }
}
