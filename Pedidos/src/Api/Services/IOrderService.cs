using Api.Dtos;

namespace Api.Services;

public interface IOrderService
{
    Task<int> CreateOrderAsync(CreateOrderDto dto);
}
