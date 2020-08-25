using System;

namespace PaymentBank
{

    public class AccountTransaction
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int AccountId { get; set; }

        public DateTime CreateDate { get; set; }

        public decimal Amount { get; set; }
    }
}
