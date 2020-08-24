using System.Collections.Generic;

namespace PaymentBank.AccountService
{
    public interface ICustomerAccountRepository
    {
        DbCustomer GetCustomer(int customerId);
        List<DbCustomerAccount> GetCustomerAccountsByCustomerId(int customerId);
        DbCustomerAccount CreateCustomerAccount(int customerId, decimal balance);
        List<DbCustomerAccount> GetCustomerAccounts();
    }
}
