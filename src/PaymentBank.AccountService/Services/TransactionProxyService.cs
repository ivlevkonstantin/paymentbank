using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaymentBank.AccountService.Services
{
    public class TransactionProxyService : ITransactionProxyService
    {
        private readonly IOptions<AccountOptions> _config;

        public TransactionProxyService(IOptions<AccountOptions> config)
        {
            _config = config;
        }

        public async Task<List<CustomerTransaction>> GetTransactions(int customerAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                var transactionResponse = await httpClient.GetStreamAsync($"{_config.Value.TransactionServiceConnectionString}/transaction/{customerAccountId}");

                var customerTransactions = await JsonSerializer.DeserializeAsync<IEnumerable<CustomerTransaction>>(transactionResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return customerTransactions.ToList();
            }
        }

        public async Task CreateTransaction(CustomerTransaction transaction)
        {
            using (var httpClient = new HttpClient())
            {
                var transactionContent = new StringContent(JsonSerializer.Serialize(transaction), Encoding.UTF8, "application/json");
                await httpClient.PostAsync($"{_config.Value.TransactionServiceConnectionString}/transaction", transactionContent);
            }
        }
    }
}
