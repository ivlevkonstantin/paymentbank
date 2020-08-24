using System.Collections.Generic;

namespace PaymentBank
{
    public class CustomerAccount
    {
        public int CustomerId { get; set; }

        public int CustomerAccountId { get; set; }

        public decimal Balance { get; set; }

        public List<CustomerTransaction> Transactions { get; set; }
    }
}
