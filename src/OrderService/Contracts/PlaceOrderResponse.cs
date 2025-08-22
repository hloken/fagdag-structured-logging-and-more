namespace OrderService.Contracts;

public record PlaceOrderResponse(
    bool Success,
    Guid OrderId,
    string ErrorMessage
);