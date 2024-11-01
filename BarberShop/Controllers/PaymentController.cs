using BarberShop.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberShop.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequest request)
        {
            try
            {
                string clientSecret = await _paymentService.CreatePaymentIntent(request.Amount, request.PaymentMethods, request.Currency);
                return Ok(new { clientSecret });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class PaymentIntentRequest
    {
        public decimal Amount { get; set; }
        public List<string> PaymentMethods { get; set; } = new List<string> { "card" }; // Default to "card"
        public string Currency { get; set; } = "brl";
    }
}
