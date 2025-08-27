namespace InventoryService.Contracts;

public record ReserveItemsResponse: IDefineEmpty<ReserveItemsResponse>
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    
    public static ReserveItemsResponse Empty { get; } = new() {Success = false, ErrorMessage = string.Empty};
};