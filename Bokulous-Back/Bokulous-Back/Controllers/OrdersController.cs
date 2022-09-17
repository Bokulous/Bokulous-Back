using Bokulous_Back.Models;
using Bokulous_Back.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bokulous_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly BokulousDbService _bokulousDbService;

        public OrdersController(BokulousDbService bokulousDbService) =>
            _bokulousDbService = bokulousDbService;

        [HttpGet("GetOrders")]
        public async Task<List<Order>> GetOrders() =>
        await _bokulousDbService.GetOrderAsync();

        [HttpGet("GetOrder/{id:length(24)}")]
        public async Task<ActionResult<Book>> GetOrder(string id)
        {
            var order = await _bokulousDbService.GetOrderAsync(id);

            if (order is null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost("AddOrder")]
        public async Task<IActionResult> AddOrder(Order newOrder)
        {
            await _bokulousDbService.CreateOrderAsync(newOrder);

            return CreatedAtAction(nameof(AddOrder), new { id = newOrder.Id }, newOrder);
        }

        [HttpPut("UpdateOrder/{id:length(24)}")]
        public async Task<IActionResult> UpdateOrder(string id, Order updatedOrder)
        {
            var order = await _bokulousDbService.GetOrderAsync(id);

            if (order is null)
            {
                return NotFound();
            }

            updatedOrder.Id = order.Id;

            await _bokulousDbService.UpdateOrderAsync(id, updatedOrder);

            return Ok();
        }

        [HttpPut("DeleteOrder/{id:length(24)}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var order = await _bokulousDbService.GetOrderAsync(id);

            if (order is null)
                return NotFound();

            return Ok();
        }
    }
}
