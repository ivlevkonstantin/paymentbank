using System.Collections.Generic;

namespace PaymentBank.TransactionService.Repository
{
    public interface ITransactionRepository
    {
        List<AccountTransaction> GetTransactions();
        List<AccountTransaction> GetTransactionsByAccountId(int accountId);
        AccountTransaction CreateTransaction(AccountTransaction transaction);
    }
}
