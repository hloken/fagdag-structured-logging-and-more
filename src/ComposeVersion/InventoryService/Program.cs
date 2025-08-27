using InventoryService;
using InventoryService.Contracts;
using Microsoft.AspNetCore.Mvc;

var inventoryRepository = new InventoryRepository
{
    Items = new()
    {
        { "ice-cream", 3 },
        { "banana", 1 }
    }
};

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
builder.AddLogging("inventory");

// Add services to the container.
builder.Services.AddProblemDetails();

// Add inventory
builder.Services.AddSingleton<InventoryRepository>(inventoryRepository);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapPost("/reserve-items", ([FromBody] ReserveItemsRequest request, InventoryRepository inventory, ILogger<Program> logger) =>
    {
        using var scope = logger.BeginScope(new Dictionary<string, object> {["OrderId"] = request.OrderId});

        return ReserveItem(request, inventory, logger) switch
        {
            true => Results.Ok(new ReserveItemsResponse {Success = true}),
            _ => Results.BadRequest(new ReserveItemsResponse {Success = false, ErrorMessage = "Out of stock"})
        };
    }
);

app.MapGet("", (InventoryRepository inventory) =>
    new GetAllResponse(inventory.Items.Select(item => new GetAllResponseItem(item.Key, item.Value)).ToArray())
);

app.Run();

bool ReserveItem(ReserveItemsRequest request, InventoryRepository inventory, ILogger<Program> logger)
{
    var success = inventory.ReserveItems(request.ItemName, request.NumberOfItems);

    if (success)
    {
        logger.LogInformation("Successfully reserved from inventory for order: {OrderId} item: {ItemName} quantity: {NumberOfItems}",
            request.OrderId, request.ItemName, request.NumberOfItems);

        return true;
    }

    logger.LogWarning("Could not reserve from inventory for order: {OrderId} item: {ItemName} quantity: {NumberOfItems}",
        request.OrderId, request.ItemName, request.NumberOfItems);

    return false;
}
