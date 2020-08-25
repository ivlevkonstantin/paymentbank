using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentBank.TransactionService.Repository;

namespace PaymentBank.TransactionService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionRepository transactionRepository, ILogger<TransactionController> logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AccountTransaction>> Get()
        {
            _logger.LogInformation("Get all customer transactions");
            return Ok(_transactionRepository.GetTransactions());
        }

        [HttpGet]
        [Route("{accountId}")]
        public ActionResult<List<AccountTransaction>> GetByAccountId(int accountId)
        {
            _logger.LogInformation($"Get transactions for a {accountId}");

            if (accountId <= 0)
            {
                return BadRequest();
            }

            List<AccountTransaction> result = _transactionRepository.GetTransactionsByAccountId(accountId);

            if (result == null)
            {
                _logger.LogWarning($"No transactions found for account {accountId}");
                return NoContent();
            }

            _logger.LogInformation($"{result.Count} transactions found for account {accountId}");
            return new JsonResult(result);
        }

        [HttpPost]
        public ActionResult<AccountTransaction> CreateTransactionForAccount(AccountTransaction customerTransaction)
        {
            if (customerTransaction == null)
            {
                return BadRequest("transaction is empty");
            }

            if (customerTransaction.AccountId <= 0)
            {
                return BadRequest("account id has invalid format");
            }

            if (customerTransaction.Amount == 0)
            {
                return BadRequest("transaction amount must not be 0");
            }

            return Ok(_transactionRepository.CreateTransaction(customerTransaction));
        }
    }
}
