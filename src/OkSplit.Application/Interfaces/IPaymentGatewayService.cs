namespace OkSplit.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<(string OrderId, decimal Amount, string Currency, string Key)> CreateOrderAsync(decimal amount, string currency = "INR");
    bool VerifyPayment(string orderId, string paymentId, string signature);
}
