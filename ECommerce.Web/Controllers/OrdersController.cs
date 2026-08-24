using System.Security.Claims;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using StripeSettings = ECommerce.Web.Settings.StripeSettings;

namespace ECommerce.Web.Controllers;

[Authorize]
public class OrdersController(
    IOrderService orderService,
    ICartService cartService,
    IOptions<StripeSettings> stripeOptions,
    ILogger<OrdersController> logger) : Controller
{
    private const string PaymentIntentSessionKey = "StripePaymentIntentId";
    private readonly StripeSettings _stripeSettings = stripeOptions.Value;

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var orders = await orderService.GetUserOrdersAsync(userId);
        return View(orders);
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var cartItems = cartService.GetCart();
        if (cartItems.Count == 0)
        {
            TempData["ErrorMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        return View(new CheckoutViewModel
        {
            Items = cartItems,
            StripePublishableKey = _stripeSettings.PublishableKey
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePaymentIntent(CheckoutViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Please correct the delivery information and try again." });
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        if (!HasStripeKeys())
        {
            logger.LogError("Stripe keys are not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Online payment is not configured. Please try again later." });
        }

        var totalAmount = await orderService.GetCheckoutTotalAsync();
        if (!totalAmount.HasValue)
        {
            return BadRequest(new { error = "Your cart contains an invalid or unavailable item." });
        }

        var amount = ToMinorUnits(totalAmount.Value);
        if (amount <= 0)
        {
            return BadRequest(new { error = "Your cart total is invalid." });
        }

        try
        {
            var paymentIntent = await new PaymentIntentService().CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = _stripeSettings.Currency,
                    PaymentMethodTypes = ["card"],
                    Metadata = new Dictionary<string, string>
                    {
                        ["UserId"] = userId
                    }
                },
                new RequestOptions { ApiKey = _stripeSettings.SecretKey });

            HttpContext.Session.SetString(PaymentIntentSessionKey, paymentIntent.Id);
            return Json(new { clientSecret = paymentIntent.ClientSecret });
        }
        catch (StripeException exception)
        {
            logger.LogError(exception, "Unable to create a Stripe PaymentIntent for user {UserId}.", userId);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Unable to start payment. Please try again." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteCheckout(CheckoutViewModel viewModel, string paymentIntentId)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(paymentIntentId))
        {
            return BadRequest(new { error = "Payment details are invalid." });
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var expectedPaymentIntentId = HttpContext.Session.GetString(PaymentIntentSessionKey);
        if (!string.Equals(paymentIntentId, expectedPaymentIntentId, StringComparison.Ordinal))
        {
            return BadRequest(new { error = "This payment does not belong to the current checkout." });
        }

        if (!HasStripeKeys())
        {
            logger.LogError("Stripe keys are not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Online payment is not configured. Please try again later." });
        }

        var totalAmount = await orderService.GetCheckoutTotalAsync();
        if (!totalAmount.HasValue)
        {
            return BadRequest(new { error = "Your cart contains an invalid or unavailable item." });
        }

        try
        {
            var paymentIntent = await new PaymentIntentService().GetAsync(
                paymentIntentId,
                requestOptions: new RequestOptions { ApiKey = _stripeSettings.SecretKey });

            if (paymentIntent.Status != "succeeded" ||
                paymentIntent.Livemode ||
                paymentIntent.Amount != ToMinorUnits(totalAmount.Value) ||
                !string.Equals(paymentIntent.Currency, _stripeSettings.Currency, StringComparison.OrdinalIgnoreCase) ||
                paymentIntent.Metadata is null ||
                !paymentIntent.Metadata.TryGetValue("UserId", out var paymentUserId) ||
                !string.Equals(paymentUserId, userId, StringComparison.Ordinal))
            {
                return BadRequest(new { error = "Payment could not be verified. Your cart has not been changed." });
            }
        }
        catch (StripeException exception)
        {
            logger.LogError(exception, "Unable to verify Stripe PaymentIntent {PaymentIntentId}.", paymentIntentId);
            return BadRequest(new { error = "Payment could not be verified. Your cart has not been changed." });
        }

        var result = await orderService.FinalizeCheckoutAsync(
            userId,
            viewModel.DeliveryAddress,
            viewModel.Phone,
            totalAmount.Value,
            paymentIntentId);
        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.ErrorMessage ?? "Unable to complete your order. Your cart has not been changed." });
        }

        HttpContext.Session.Remove(PaymentIntentSessionKey);
        TempData["SuccessMessage"] = "Your payment was successful and your order has been placed.";
        return Json(new { redirectUrl = Url.Action(nameof(Index)) });
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Forbid();
        }

        var order = await orderService.GetUserOrderDetailsAsync(id, userId);
        return order is null ? NotFound() : View(order);
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private bool HasStripeKeys()
    {
        return !string.IsNullOrWhiteSpace(_stripeSettings.SecretKey) &&
               !string.IsNullOrWhiteSpace(_stripeSettings.PublishableKey);
    }

    private static long ToMinorUnits(decimal amount)
    {
        return decimal.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
    }
}
