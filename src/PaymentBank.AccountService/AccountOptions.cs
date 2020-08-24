namespace PaymentBank.AccountService
{
    public class AccountOptions
    {
        public const string AccountConfig = nameof(AccountConfig);

        public string TransactionServiceConnectionString { get; set; }
    }
}
