using InventoryApi.DTOs;
using InventoryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<OrderReadDto>> PlaceOrder(OrderCreateDto dto)
        {
            try
            {
                var result = await _orderService.PlaceOrderAsync(dto);
                return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
            }
            catch (InsufficientStockException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderReadDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound($"Order {id} not found.");
            return Ok(order);
        }

        // GET: api/orders/customer/3
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<OrderReadDto>>> GetOrdersByCustomer(int customerId)
        {
            return Ok(await _orderService.GetOrdersByCustomerAsync(customerId));
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<List<OrderReadDto>>> GetAllOrders()
        {
            return Ok(await _orderService.GetAllOrdersAsync());
        }
    }
}
