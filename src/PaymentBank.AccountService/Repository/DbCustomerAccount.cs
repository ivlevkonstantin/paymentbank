namespace PaymentBank.AccountService
{
    public class DbCustomerAccount
    {
        public int CustomerId { get; set; }
        public int CustomerAccountId { get; set; }
        public decimal Balance { get; set; }
    }
}
