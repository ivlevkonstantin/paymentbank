using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentBank.TransactionService.Repository
{
    public class TransactionInMemoryRepository : ITransactionRepository
    {
        private readonly Dictionary<int, List<AccountTransaction>> _accountTransactions = new Dictionary<int, List<AccountTransaction>>();

        public TransactionInMemoryRepository()
        {
            _accountTransactions.Add(1, new List<AccountTransaction>
            {
                new AccountTransaction
                {
                    AccountId = 1,
                    Amount = 7,
                    CreateDate = new DateTime(2018, 1, 13),
                    Id = 1,
                    CustomerId = 1
                },
                new AccountTransaction
                {
                    AccountId = 1,
                    Amount = 3,
                    CreateDate = new DateTime(2018, 3, 29),
                    Id = 2,
                    CustomerId = 1
                }
            });

            _accountTransactions.Add(2, new List<AccountTransaction>
            {
                new AccountTransaction
                {
                    AccountId = 2,
                    Amount = 20,
                    CreateDate = new DateTime(2019, 2, 19),
                    Id = 3,
                    CustomerId = 1
                }
            });

            _accountTransactions.Add(3, new List<AccountTransaction>
            {
                new AccountTransaction
                {
                    AccountId = 3,
                    Amount = 18,
                    CreateDate = new DateTime(2020, 5, 26),
                    Id = 4,
                    CustomerId = 2
                },
                new AccountTransaction
                {
                    AccountId = 3,
                    Amount = 12,
                    CreateDate = new DateTime(2020, 5, 26),
                    Id = 5,
                    CustomerId = 2
                },
            });
        }

        public List<AccountTransaction> GetTransactions()
        {
            return _accountTransactions.SelectMany(c => c.Value).ToList();
        }

        public List<AccountTransaction> GetTransactionsByAccountId(int accountId)
        {
            _accountTransactions.TryGetValue(accountId, out List<AccountTransaction> transactions);
            return transactions;
        }

        public AccountTransaction CreateTransaction(AccountTransaction transaction)
        {
            var maxTransactionId = _accountTransactions.SelectMany(c => c.Value).Select(c => c.Id).Max();
            transaction.Id = maxTransactionId;
            if (_accountTransactions.ContainsKey(transaction.AccountId))
            {
                _accountTransactions[transaction.AccountId].Add(transaction);
            }
            else
            {
                _accountTransactions[transaction.AccountId] = new List<AccountTransaction> { transaction };
            }

            return transaction;
        }
    }
}
