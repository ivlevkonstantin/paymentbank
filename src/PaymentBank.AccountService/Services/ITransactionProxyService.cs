using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentBank.AccountService.Services
{
    public interface ITransactionProxyService
    {
        Task<List<CustomerTransaction>> GetTransactions(int customerAccountId);
        Task CreateTransaction(CustomerTransaction transaction);
    }
}
