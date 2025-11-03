namespace BlazorApp4.Domain
{   //Enum for different transactions
    public enum TransactionType
    {
        Deposit,
        Withdraw,
        TransferIn,
        TransferOut
    }

    /// <summary>
    /// Represents a financial transaction, including details such as source and destination accounts, amount, type, and
    /// resulting balance.
    /// </summary>
    /// <remarks>A Transaction instance records the transfer of funds between accounts, or other financial
    /// operations, along with metadata such as the time of occurrence and transaction type. The class is typically used
    /// to track account activity and maintain an audit trail. All properties should be set to accurately reflect the
    /// transaction's context and outcome.</remarks>
    public class Transaction
    {
        public Guid Id { get; set; }  = Guid.NewGuid();
        public Guid? FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public decimal Amount { get; set; } 
        public DateTime TimeStamp { get; set; } 
        public TransactionType TransactionType { get; set; }
        public decimal BalanceAfter { get; set; }
    }
    
}
