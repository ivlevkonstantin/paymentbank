using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaymentBank.AccountService.Services
{
    public class TransactionProxyService : ITransactionProxyService
    {
        private readonly IOptions<AccountOptions> _config;
        private readonly ILogger<TransactionProxyService> _logger;

        public TransactionProxyService(IOptions<AccountOptions> config, ILogger<TransactionProxyService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<List<AccountTransaction>> GetTransactions(int customerAccountId)
        {
            using (var httpClient = new HttpClient())
            {
                var transactionResponse = await httpClient.GetAsync($"{_config.Value.TransactionServiceConnectionString}/transaction/{customerAccountId}");

                if (!transactionResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Request to the Transaction service returned {transactionResponse.StatusCode}");
                    throw new Exception("Can not reach out to transaction service");
                }

                if (transactionResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return new List<AccountTransaction>();
                }

                using (var responseStream = await transactionResponse.Content.ReadAsStreamAsync())
                {
                    IEnumerable<AccountTransaction> customerTransactions = await JsonSerializer.DeserializeAsync<IEnumerable<AccountTransaction>>(responseStream, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return customerTransactions.ToList();
                }
            }
        }

        public async Task CreateTransaction(AccountTransaction transaction)
        {
            using (var httpClient = new HttpClient())
            {
                var transactionContent = new StringContent(JsonSerializer.Serialize(transaction), Encoding.UTF8, "application/json");
                await httpClient.PostAsync($"{_config.Value.TransactionServiceConnectionString}/transaction", transactionContent);
            }
        }
    }
}
