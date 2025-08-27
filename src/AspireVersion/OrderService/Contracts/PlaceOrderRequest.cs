namespace OrderService.Contracts;

public record PlaceOrderRequest(
    string ItemName,
    int NumberOfItems);