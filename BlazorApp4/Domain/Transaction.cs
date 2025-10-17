namespace BlazorApp4.Domain
{
    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer
    }
    public class Transaction
    {
        public Guid Id { get; set; }  = Guid.NewGuid();
        public Guid? FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public decimal Amount { get; set; } 
        public DateTime TimeStamp { get; set; } 
        public TransactionType TransactionType { get; set; }
        public decimal BalanceAfterTransaction { get; set; }


    }
    
}
