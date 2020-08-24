using System.Collections.Generic;

namespace PaymentBank.TransactionService.Repository
{
    public interface ITransactionRepository
    {
        List<CustomerTransaction> GetTransactions();
        List<CustomerTransaction> GetTransactionsByAccountId(int accountId);
        CustomerTransaction CreateTransaction(CustomerTransaction transaction);
    }
}
