
using System.Collections.Generic;

namespace BlazorApp4.Interfaces
{
    public interface IAccountService
    {
        Task<IBankAccount> CreateAccount(string name, AccountType accountType, Currency currency, decimal initialBalance);
        Task<List<IBankAccount>> GetAccounts();
        Task<List<Transaction>> GetTransactionsHistoryAsync();
        Task DepositAsync(Guid accountId, decimal amount);
        Task RemoveAccountAynsc(Guid accountId);


    }
}
