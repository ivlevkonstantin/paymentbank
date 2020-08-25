using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentBank.AccountService.Services
{
    public interface ITransactionProxyService
    {
        Task<List<AccountTransaction>> GetTransactions(int customerAccountId);
        Task CreateTransaction(AccountTransaction transaction);
    }
}
