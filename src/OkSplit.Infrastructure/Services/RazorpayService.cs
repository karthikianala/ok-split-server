using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using OkSplit.Application.Interfaces;
using Razorpay.Api;

namespace OkSplit.Infrastructure.Services;

public class RazorpayService : IPaymentGatewayService
{
    private readonly string _keyId;
    private readonly string _keySecret;

    public RazorpayService(IConfiguration configuration)
    {
        _keyId = configuration["Razorpay:KeyId"]!;
        _keySecret = configuration["Razorpay:KeySecret"]!;
    }

    public async Task<(string OrderId, decimal Amount, string Currency, string Key)> CreateOrderAsync(
        decimal amount, string currency = "INR")
    {
        var client = new RazorpayClient(_keyId, _keySecret);

        var options = new Dictionary<string, object>
        {
            { "amount", (int)(amount * 100) }, // Razorpay uses paise
            { "currency", currency },
            { "payment_capture", 1 }
        };

        var order = client.Order.Create(options);

        string orderId = order["id"].ToString();

        return await Task.FromResult((
            OrderId: orderId,
            Amount: amount,
            Currency: currency,
            Key: _keyId
        ));
    }

    public bool VerifyPayment(string orderId, string paymentId, string signature)
    {
        var payload = $"{orderId}|{paymentId}";
        var expectedSignature = ComputeHmacSha256(payload, _keySecret);
        return string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
