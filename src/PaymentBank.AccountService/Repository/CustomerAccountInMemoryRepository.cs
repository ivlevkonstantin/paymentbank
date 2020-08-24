using System.Collections.Generic;
using System.Linq;

namespace PaymentBank.AccountService
{
    public class CustomerAccountInMemoryRepository : ICustomerAccountRepository
    {
        private readonly Dictionary<int, List<DbCustomerAccount>> _customerAccounts = new Dictionary<int, List<DbCustomerAccount>>(); //customerId, customerAccount
        private readonly Dictionary<int, DbCustomer> _customers = new Dictionary<int, DbCustomer>(); //customerId, customerAccount

        public CustomerAccountInMemoryRepository()
        {
            _customerAccounts.Add(1, new List<DbCustomerAccount> {
                new DbCustomerAccount
                {
                    CustomerAccountId = 1,
                    CustomerId = 1,
                    Balance = 10
                },
                new DbCustomerAccount
                {
                    CustomerAccountId = 2,
                    CustomerId = 1,
                    Balance = 20
                }
            });

            _customerAccounts.Add(2, new List<DbCustomerAccount> {
                new DbCustomerAccount
                {
                    CustomerAccountId = 3,
                    CustomerId = 2,
                    Balance = 30
                }
            });

            _customers.Add(1, new DbCustomer
            {
                CustomerId = 1,
                Name = "John",
                Surname = "Sidorov"
            });

            _customers.Add(2, new DbCustomer
            {
                CustomerId = 2,
                Name = "Ivan",
                Surname = "Ivanov"
            });
        }

        public DbCustomerAccount CreateCustomerAccount(int customerId, decimal balance)
        {
            int maxAccountId = _customerAccounts.SelectMany(c => c.Value).Select(c => c.CustomerAccountId).Max();
            var newAccount = new DbCustomerAccount
            {
                CustomerAccountId = maxAccountId + 1,
                Balance = balance,
                CustomerId = customerId
            };

            _customerAccounts[customerId].Add(newAccount);

            return newAccount;
        }

        public DbCustomer GetCustomer(int customerId)
        {
            _customers.TryGetValue(customerId, out DbCustomer customer);
            return customer;
        }

        public List<DbCustomerAccount> GetCustomerAccounts()
        {
            return _customerAccounts.SelectMany(c => c.Value).ToList();
        }

        public List<DbCustomerAccount> GetCustomerAccountsByCustomerId(int customerId)
        {
            _customerAccounts.TryGetValue(customerId, out List<DbCustomerAccount> accounts);
            return accounts;
        }
    }
}
