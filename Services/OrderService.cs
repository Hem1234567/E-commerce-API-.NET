using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderReadDto> PlaceOrderAsync(OrderCreateDto dto)
        {
            var customer = await _context.Customers.FindAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException($"Customer {dto.CustomerId} not found.");

            if (dto.Items == null || dto.Items.Count == 0)
                throw new ArgumentException("Order must contain at least one item.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId)
                    ?? throw new KeyNotFoundException($"Product {item.ProductId} not found.");

                if (product.StockQuantity < item.Quantity)
                    throw new InsufficientStockException(
                        $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}.");

                product.StockQuantity -= item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                total += orderItem.Quantity * orderItem.UnitPrice;
                order.OrderItems.Add(orderItem);
            }

            order.TotalAmount = total;
            order.Status = OrderStatus.Confirmed;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await MapToReadDtoAsync(order.Id) 
                ?? throw new InvalidOperationException("Order was created but could not be reloaded.");
        }

        public async Task<OrderReadDto?> GetOrderByIdAsync(int id)
        {
            return await MapToReadDtoAsync(id);
        }

        public async Task<List<OrderReadDto>> GetOrdersByCustomerAsync(int customerId)
        {
            var orderIds = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Select(o => o.Id)
                .ToListAsync();

            var results = new List<OrderReadDto>();
            foreach (var id in orderIds)
            {
                var dto = await MapToReadDtoAsync(id);
                if (dto != null) results.Add(dto);
            }
            return results;
        }

        public async Task<List<OrderReadDto>> GetAllOrdersAsync()
        {
            var orderIds = await _context.Orders.Select(o => o.Id).ToListAsync();
            var results = new List<OrderReadDto>();
            foreach (var id in orderIds)
            {
                var dto = await MapToReadDtoAsync(id);
                if (dto != null) results.Add(dto);
            }
            return results;
        }

        private async Task<OrderReadDto?> MapToReadDtoAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            return new OrderReadDto
            {
                Id = order.Id,
                CustomerName = order.Customer?.FullName ?? "Unknown",
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemReadDto
                {
                    ProductName = oi.Product?.Name ?? "Unknown",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
        }
    }
}
