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
        public Guid FromAccountId { get; set; } = Guid.NewGuid();
        public Guid ToAccountId { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; } = new decimal();
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public TransactionType TransactionType { get; set; } = new TransactionType();

        
    }
    
}
