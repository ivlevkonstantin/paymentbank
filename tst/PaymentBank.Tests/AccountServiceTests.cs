using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using PaymentBank.AccountService;
using PaymentBank.AccountService.Controllers;
using PaymentBank.AccountService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PaymentBank.Tests
{
    [TestFixture]
    public class AccountServiceTests
    {
        private Mock<ICustomerAccountRepository> _customerAccountRepositoryMock;
        private Mock<ITransactionProxyService> _transactionProxyServiceMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ILogger<AccountController>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _customerAccountRepositoryMock = new Mock<ICustomerAccountRepository>(MockBehavior.Strict);
            _transactionProxyServiceMock = new Mock<ITransactionProxyService>(MockBehavior.Strict);
            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<AccountController>>(MockBehavior.Loose);
        }

        [Test]
        public void Get_Basic_Ok()
        {
            //Arrange
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomerAccounts())
                .Returns(new List<DbCustomerAccount> { new DbCustomerAccount {
                    Balance = 1, CustomerAccountId = 1, CustomerId = 1 } });

            //Act
            var result = target.Get();

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf(typeof(OkObjectResult), result.Result);
                Assert.AreEqual(1, ((List<DbCustomerAccount>)((OkObjectResult)result.Result).Value).Count());
            });
        }

        [Test]
        public void Get_Exception_InternalServerError()
        {
            //Arrange
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomerAccounts())
                .Throws<Exception>();

            //Act
            var result = target.Get();

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<StatusCodeResult>(result.Result);
                Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((StatusCodeResult)result.Result).StatusCode);
            });
        }

        [Test]
        public async Task GetByCustomerId_CustomerIdNegative_BadRequest()
        {
            //Arrange
            var target = CreateTarget();

            //Act
            var result = await target.GetByCustomerId(-1);

            //Assert
            Assert.IsInstanceOf<BadRequestResult>(result.Result);
        }

        [Test]
        public async Task GetByCustomerId_UnexpectedError_InternalServerErrorCode()
        {
            //Arrange
            const int customerId = 1;
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomerAccountsByCustomerId(customerId))
                .Throws<Exception>();
            var target = CreateTarget();

            //Act
            var result = await target.GetByCustomerId(customerId);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<StatusCodeResult>(result.Result);
                Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((StatusCodeResult)result.Result).StatusCode);
            });
        }

        [Test]
        public async Task GetByCustomerId_CustomerDoesNotExist_NotFound()
        {
            //Arrange
            const int customerId = 1;
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomerAccountsByCustomerId(customerId))
                .Returns((List<DbCustomerAccount>)null);
            _mapperMock
                .Setup(c => c.Map<List<CustomerAccount>>(It.IsAny<List<DbCustomerAccount>>()))
                .Returns((List<CustomerAccount>)null);

            //Act
            var result = await target.GetByCustomerId(customerId);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<NotFoundResult>(result.Result);
                _customerAccountRepositoryMock.VerifyAll();
                _mapperMock.VerifyAll();
            });
        }

        [Test]
        public async Task GetByCustomerId_CustomerAccountExist_RequestTransactionPerAccount()
        {
            //Arrange
            const int customerId = 1;
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomerAccountsByCustomerId(customerId))
                .Returns((List<DbCustomerAccount>)null);
            _mapperMock
                .Setup(c => c.Map<List<CustomerAccount>>(It.IsAny<List<DbCustomerAccount>>()))
                .Returns(new List<CustomerAccount> { 
                    new CustomerAccount
                    {
                        CustomerId = 1,
                        CustomerAccountId = 2
                    },
                    new CustomerAccount
                    {
                        CustomerId = 1,
                        CustomerAccountId = 3
                    }
                });

            _transactionProxyServiceMock
                .SetupSequence(c => c.GetTransactions(It.IsAny<int>()))
                .Returns(Task.FromResult(new List<AccountTransaction> { new AccountTransaction() }))
                .Returns(Task.FromResult(new List<AccountTransaction> { new AccountTransaction() }));

            //Act
            var result = await target.GetByCustomerId(customerId);

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<OkObjectResult>(result.Result);
                Assert.AreEqual(2, ((List<CustomerAccount>)((OkObjectResult)result.Result).Value).Count());
                _customerAccountRepositoryMock.VerifyAll();
                _transactionProxyServiceMock.VerifyAll();
                _mapperMock.VerifyAll();
            });
        }

        [Test]
        public async Task OpenAccount_InvalidCustomerId_BadRequest()
        {
            //Arrange
            var target = CreateTarget();

            //Act
            var result = await target.OpenAccount(new CustomerAccountCreateRequest
            {
                CustomerId = -1
            });

            //Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task OpenAccount_InvalidInitialCredit_BadRequest()
        {
            //Arrange
            var target = CreateTarget();
            //Act
            var result = await target.OpenAccount(new CustomerAccountCreateRequest
            {
                InitialCredit = -1,
                CustomerId = 1
            });

            //Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public async Task OpenAccount_CustomerDoesNotExist_BadRequest()
        {
            //Arrange
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomer(It.IsAny<int>()))
                .Returns((DbCustomer) null);

            //Act
            var result = await target.OpenAccount(new CustomerAccountCreateRequest
            {
                InitialCredit = 0,
                CustomerId = 1
            });

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
                _customerAccountRepositoryMock.VerifyAll();
            });
        }

        [Test]
        public async Task OpenAccount_EmptyInitialCredit_NotTransactionCreated()
        {
            //Arrange
            int customerId = Math.Abs(Guid.NewGuid().GetHashCode());
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomer(customerId))
                .Returns(new DbCustomer
                {
                    CustomerId = customerId
                });

            var newCustomerAccount = new DbCustomerAccount
            {
                CustomerId = customerId
            };

            _customerAccountRepositoryMock
                .Setup(c => c.CreateCustomerAccount(customerId, 0))
                .Returns(newCustomerAccount);

            _mapperMock
                .Setup(c => c.Map<CustomerAccount>(newCustomerAccount))
                .Returns(new CustomerAccount());

            //Act
            var result = await target.OpenAccount(new CustomerAccountCreateRequest
            {
                InitialCredit = 0,
                CustomerId = customerId
            });

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<OkObjectResult>(result.Result);
                _customerAccountRepositoryMock.VerifyAll();
                _transactionProxyServiceMock
                    .Verify(c => c.CreateTransaction(It.IsAny<AccountTransaction>()), Times.Never);
                Assert.IsInstanceOf<OkObjectResult>(result.Result);
            });
        }

        [Test]
        public async Task OpenAccount_PositiveInitialCredit_TransactionCreated()
        {
            //Arrange
            const decimal balance = 123;
            int customerId = Math.Abs(Guid.NewGuid().GetHashCode());
            int customerAccountId = Math.Abs(Guid.NewGuid().GetHashCode());
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomer(customerId))
                .Returns(new DbCustomer
                {
                    CustomerId = customerId
                });

            var newCustomerAccount = new DbCustomerAccount
            {
                CustomerId = customerId,
                CustomerAccountId = customerAccountId
            };

            _customerAccountRepositoryMock
                .Setup(c => c.CreateCustomerAccount(customerId, balance))
                .Returns(newCustomerAccount);

            _mapperMock
                .Setup(c => c.Map<CustomerAccount>(newCustomerAccount))
                .Returns(new CustomerAccount());

            AccountTransaction createdTransaction = null;
            _transactionProxyServiceMock
                .Setup(c => c.CreateTransaction(It.IsAny<AccountTransaction>()))
                .Returns(Task.CompletedTask)
                .Callback((AccountTransaction tran) => { createdTransaction = tran; });

            //Act
            var result = await target.OpenAccount(new CustomerAccountCreateRequest
            {
                InitialCredit = balance,
                CustomerId = customerId
            });

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<OkObjectResult>(result.Result);
                _customerAccountRepositoryMock.VerifyAll();
                _transactionProxyServiceMock.VerifyAll();
                Assert.IsInstanceOf<OkObjectResult>(result.Result);
                Assert.AreEqual(customerId, createdTransaction.CustomerId);
                Assert.AreEqual(customerAccountId, createdTransaction.AccountId);
                Assert.AreEqual(balance, createdTransaction.Amount);
            });
        }

        [Test]
        public async Task OpenAccount_Exception_InternalServerError()
        {
            //Arrange
            int customerId = Math.Abs(Guid.NewGuid().GetHashCode());
            var target = CreateTarget();
            _customerAccountRepositoryMock
                .Setup(c => c.GetCustomer(customerId))
                .Throws<Exception>();

            //Act
            var result = await target.OpenAccount(new CustomerAccountCreateRequest
            {
                InitialCredit = 0,
                CustomerId = customerId
            });

            //Assert
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<StatusCodeResult>(result.Result);
                Assert.AreEqual((int)HttpStatusCode.InternalServerError, ((StatusCodeResult)result.Result).StatusCode);
            });
        }

        private AccountController CreateTarget()
        {
            return new AccountController(_customerAccountRepositoryMock.Object, _transactionProxyServiceMock.Object, _mapperMock.Object, _loggerMock.Object);
        }
    }
}
