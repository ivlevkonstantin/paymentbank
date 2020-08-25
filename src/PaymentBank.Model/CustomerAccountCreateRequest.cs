using Swashbuckle.AspNetCore.Annotations;

namespace PaymentBank
{
    [SwaggerSchema("New account request parameters", Required = new[] { "CustomerId" })]
    public class CustomerAccountCreateRequest
    {
        [SwaggerSchema("The customer identifier")]
        public int CustomerId { get; set; }

        [SwaggerSchema("The initial credit for a new customer account. A corresponding transaction will be created in case of positive value")]
        public decimal InitialCredit { get; set; }
    }
}
