using System.Collections.Generic;

namespace PaymentBank
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public decimal Balance { get; set; }

        public List<CustomerAccount> customerAccounts { get; set; }
    }
}
