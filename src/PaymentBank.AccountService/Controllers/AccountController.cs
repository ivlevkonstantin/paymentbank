using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentBank.AccountService.Services;

namespace PaymentBank.AccountService.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ICustomerAccountRepository _customerAccountRepository;
        private readonly ITransactionProxyService _transactionProxyService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            ICustomerAccountRepository customerAccountRepository,
            ITransactionProxyService transactionProxyService,
            IMapper mapper,
            ILogger<AccountController> logger)
        {
            _customerAccountRepository = customerAccountRepository;
            _transactionProxyService = transactionProxyService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("account")]
        public ActionResult<IEnumerable<CustomerAccount>> Get()
        {
            _logger.LogInformation("Get all customer accounts");
            var customerAccounts = _customerAccountRepository.GetCustomerAccounts();
            return Ok(customerAccounts);
        }

        [HttpGet("account/{customerId}")]
        public async Task<ActionResult<IEnumerable<CustomerAccount>>> GetByCustomerId(int customerId)
        {
            _logger.LogInformation($"Get accounts for a {customerId}");

            try
            {
                if (customerId <= 0)
                {
                    _logger.LogWarning($"Invalid customer id: {customerId}");
                    return BadRequest();
                }

                List<CustomerAccount> customerAccounts = _mapper.Map<List<CustomerAccount>>(_customerAccountRepository.GetCustomerAccountsByCustomerId(customerId));

                if (customerAccounts == null)
                {
                    _logger.LogWarning($"No accounts found for customer {customerId}");
                    return NotFound();
                }

                _logger.LogInformation($"{customerAccounts.Count} accounts found for customer {customerId}");

                foreach (var customerAccount in customerAccounts)
                {
                    customerAccount.Transactions = await _transactionProxyService.GetTransactions(customerAccount.CustomerAccountId);
                }

                return Ok(customerAccounts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in {nameof(GetByCustomerId)} with customerId {customerId}. Details: {ex}");
                return StatusCode(500);
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<DbCustomer>> GetCustomerInfoByCustomerId(int customerId)
        {
            _logger.LogInformation($"Get customer info for a {customerId}");

            if (customerId <= 0)
            {
                _logger.LogWarning($"Invalid customer id: {customerId}");
                return BadRequest();
            }

            Customer customer = _mapper.Map<Customer>(_customerAccountRepository.GetCustomer(customerId));

            if (customer == null)
            {
                _logger.LogWarning($"No customer info found for customer {customerId}");
                return NotFound();
            }

            List<CustomerAccount> customerAccounts = _mapper.Map<List<CustomerAccount>>(_customerAccountRepository.GetCustomerAccountsByCustomerId(customerId));

            if (customerAccounts == null)
            {
                return Ok(customer);
            }

            customer.Balance = customerAccounts.Sum(c => c.Balance);

            foreach (var customerAccount in customerAccounts)
            {
                customerAccount.Transactions = await _transactionProxyService.GetTransactions(customerAccount.CustomerAccountId);
            }

            customer.customerAccounts = customerAccounts;

            return Ok(customer);
        }

        [HttpPost("accountcreaterequest")]
        public async Task<ActionResult<CustomerAccount>> OpenAccount(CustomerAccountCreateRequest customerAccountCreateRequest)
        {
            if (customerAccountCreateRequest.CustomerId <= 0)
            {
                return BadRequest("Invalid customer Id");
            }

            if (customerAccountCreateRequest.InitialCredit < 0)
            {
                return BadRequest("Initial credit could not be negative");
            }

            var customer = _customerAccountRepository.GetCustomer(customerAccountCreateRequest.CustomerId);
            if (customer == null)
            {
                return BadRequest($"Customer with id {customerAccountCreateRequest.CustomerId} does not exist");
            }

            var newAccount = _customerAccountRepository.CreateCustomerAccount(customerAccountCreateRequest.CustomerId, customerAccountCreateRequest.InitialCredit);

            if (customerAccountCreateRequest.InitialCredit != 0)
            {
                var newTransaction = new CustomerTransaction
                {
                    AccountId = newAccount.CustomerAccountId,
                    Amount = customerAccountCreateRequest.InitialCredit,
                    CreateDate = DateTime.UtcNow,
                    CustomerId = customerAccountCreateRequest.CustomerId
                };

                await _transactionProxyService.CreateTransaction(newTransaction);
            }

            return Ok(_mapper.Map<CustomerAccount>(newAccount));
        }
    }
}
