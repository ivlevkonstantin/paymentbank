using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentBank.AccountService.Services;
using Swashbuckle.AspNetCore.Annotations;

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

        [Produces("application/json")]
        [SwaggerResponse(200, "A list of all customer accounts successfully retrived")]
        [SwaggerOperation(
            Summary = "Returns a list of all customer accounts",
            Description = "Returns a list of all customer accounts without transactions.",
            OperationId = "GetCustomerAccounts"
        )]
        [HttpGet("account")]
        public ActionResult<IEnumerable<CustomerAccount>> Get()
        {
            _logger.LogInformation("Get all customer accounts");
            var customerAccounts = _customerAccountRepository.GetCustomerAccounts();
            return Ok(customerAccounts);
        }

        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Returns a list of customer accounts for a particular customer",
            Description = "Returns a list of customer accounts for a particular customer",
            OperationId = "GetCustomerAccountsByCustomerId"
        )]
        [SwaggerResponse(404, "Customer does not exist")]
        [SwaggerResponse(400, "Invalid customer id")]
        [SwaggerResponse(200, "A list of customer accounts successfully retrieved")]
        [HttpGet("account/{customerId}")]
        public async Task<ActionResult<IEnumerable<CustomerAccount>>> GetByCustomerId(
            [SwaggerParameter("Customer identifier", Required = true)] int customerId)
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

        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Returns an extended customer information",
            Description = "Returns an extended customer information",
            OperationId = "GetCustomerInfoByCustomerId"
        )]
        [SwaggerResponse(404, "Customer does not exist")]
        [SwaggerResponse(400, "Invalid customer id")]
        [SwaggerResponse(200, "Customer info")]
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<Customer>> GetCustomerInfoByCustomerId(
            [SwaggerParameter("Customer identifier", Required = true)] int customerId)
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

        [Produces("application/json")]
        [SwaggerOperation(
            Summary = "Creates a new account for a customer",
            Description = @"Creates a new account for a selected customer.
                            Creates a new transaction for the account in case of positive initial credit",
            OperationId = "GetCustomerInfoByCustomerId"
        )]
        [SwaggerResponse(400, "Invalid customer Id")]
        [SwaggerResponse(400, "Initial credit could not be negative")]
        [SwaggerResponse(400, "A customer does not exist")]
        [SwaggerResponse(200, "Brand new customer account without transactions")]
        [HttpPost("accountcreaterequest")]
        public async Task<ActionResult<CustomerAccount>> OpenAccount(
            [SwaggerParameter("New account request parameters", Required = true)] CustomerAccountCreateRequest customerAccountCreateRequest)
        {
            if (customerAccountCreateRequest.CustomerId <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Detail = "Invalid customer Id"
                });
            }

            if (customerAccountCreateRequest.InitialCredit < 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Detail = "Initial credit could not be negative"
                });
            }

            var customer = _customerAccountRepository.GetCustomer(customerAccountCreateRequest.CustomerId);
            if (customer == null)
            {
                return BadRequest($"Customer with id {customerAccountCreateRequest.CustomerId} does not exist");
            }

            var newAccount = _customerAccountRepository.CreateCustomerAccount(customerAccountCreateRequest.CustomerId, customerAccountCreateRequest.InitialCredit);

            if (customerAccountCreateRequest.InitialCredit != 0)
            {
                var newTransaction = new AccountTransaction
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
