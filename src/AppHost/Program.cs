var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.InventoryService>("inventoryservice");
builder.AddProject<Projects.OrderService>("orderservice");

builder.Build().Run();
