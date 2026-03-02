namespace Api.Dtos
{
    public record CreateOrderItemDto(int ProductId, int Quantity);
    public record CreateOrderDto(string CustomerName, List<CreateOrderItemDto> Items);
}
