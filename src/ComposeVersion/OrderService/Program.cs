using Microsoft.AspNetCore.Mvc;
using OrderService;
using OrderService.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
builder.AddLogging("order");

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddHttpClient<InventoryApiClient>(client =>
{
    client.BaseAddress = new("http://localhost:5002");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Place Order
app.MapPost("/place-order", async ([FromBody] PlaceOrderRequest request, InventoryApiClient inventoryApi,
    HttpContext ctx, ILogger<Program> logger) =>
{
    var orderId = Guid.NewGuid();
    using var scope = logger.BeginScope(new Dictionary<string, object> { ["OrderId"] = orderId});

    // Validation
    if (string.IsNullOrWhiteSpace(request.ItemName))
    {
        logger.LogError("Order placed with empty item-name: {RequestContent}", request);
        return Results.BadRequest(new PlaceOrderResponse(false, Guid.Empty, "Order placed with empty item-name"));
    }

    logger.LogInformation("Received new order {OrderId} for item: {ItemName} quantity: {NumberOfItems}",
        orderId, request.ItemName, request.NumberOfItems);

    // Checking with inventory
    var success = await inventoryApi.ReserveItems(request.ItemName, request.NumberOfItems, orderId, ctx.RequestAborted);
    if (!success)
    {
        logger.LogInformation(
            "Cannot place order, inventory-service could not reserve items for {OrderId}, item: {ItemName} Quantity: {NumberOfItems}",
            orderId, request.ItemName, request.NumberOfItems);
        return Results.BadRequest(new PlaceOrderResponse(false, orderId, "Inventory says no"));
    }

    logger.LogInformation("Successfully placed order {OrderId} for item: {ItemName}, quantity: {NumberOfItems}",
        orderId, request.ItemName, request.NumberOfItems);

    return Results.Accepted(value: new PlaceOrderResponse(true, orderId, string.Empty));
});

app.Run();