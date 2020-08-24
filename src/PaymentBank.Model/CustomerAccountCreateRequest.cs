namespace PaymentBank
{
    public class CustomerAccountCreateRequest
    {
        public int CustomerId { get; set; }

        public decimal InitialCredit { get; set; }
    }
}
