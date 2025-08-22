var builder = DistributedApplication.CreateBuilder(args);

var inventoryService = builder.AddProject<Projects.InventoryService>("inventoryservice");

builder.AddProject<Projects.OrderService>("orderservice")
    .WithReference(inventoryService);

builder.Build().Run();
