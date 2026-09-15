using InventoryApi.DTOs;

namespace InventoryApi.Services
{
    public interface IOrderService
    {
        Task<OrderReadDto> PlaceOrderAsync(OrderCreateDto dto);
        Task<OrderReadDto?> GetOrderByIdAsync(int id);
        Task<List<OrderReadDto>> GetOrdersByCustomerAsync(int customerId);
        Task<List<OrderReadDto>> GetAllOrdersAsync();
    }

    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(string message) : base(message) { }
    }
}
