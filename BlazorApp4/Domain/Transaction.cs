namespace BlazorApp4.Domain
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.Now;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
    }
    public enum TransactionType
    {
        Insättning,
        Uttag,
        Överföring
    }

    
}