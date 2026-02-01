using Colibri.ConnectNetwork.Services.Abstract;
using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Enum;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Контроллер для управления клиентскими заказами
    /// </summary>
    [Route("order")]
    [ApiController]
    public class ClientOrderController(IClientOrderService clientOrder, ILoggerService logger, IHttpConnectService httpConnect, IAccountService accountService) : Controller
    {
        private readonly IClientOrderService _clientOrder = clientOrder;
        private readonly ILoggerService _logger = logger;
        private readonly IHttpConnectService _httpConnect = httpConnect;
        private readonly IAccountService _accountService = accountService;
        /// <summary>
        /// Создание нового продукта.
        /// </summary>
        /// <param name="product">Объект продукта, представляющий данные о новом продукте.</param>
        /// <returns>Результат выполнения операции на создание продукта. Возвращает <see cref="OkResult"/> при успешном ответе или <see cref="BadRequestResult"/> при ошибке.</returns>
        [Authorize]
        [HttpPost("creatproduct")]
        public async Task<IActionResult> CreatProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest();
                }
                await _clientOrder.CreadProductAsync(product);

                _logger.LogMessage(User, $"Добавлен продукт в базу данных -- [{product.Name}]", LogLevel.Information);

                return Ok("succes");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Выборка всех товаров
        /// </summary>
        /// <returns>Список всех товаров</returns>
        [Authorize]
        [HttpGet("getproducts")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _clientOrder.GetProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Выборка товара по идентификатору
        /// </summary>
        /// <param name="productId">Идентификатор продукта</param>
        /// <returns>Товар соответствующий заданному пораметру </returns>
        [Authorize]
        [HttpGet("getproductid/{productId}")]
        public async Task<IActionResult> GetProductId(int productId)
        {
            try
            {
                var product = await _clientOrder.GetProductByIdAsync(productId);
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }
        /// <summary>
        /// Размещение нового заказа.
        /// </summary>
        /// <param name="orderModel">Модель, представляющая данные о новом заказе.</param>
        /// <returns>Результат выполнения операции на создание продукта. Возвращает <see cref="OkResult"/> при успешном ответе или <see cref="BadRequestResult"/> при ошибке.</returns>
        [Authorize]
        [HttpPost("placeorder")]
        public async Task<IActionResult> PlaceOrder(OrderModel orderModel)
        {
            try
            {
                if (orderModel == null)
                {
                    return BadRequest("Order data is required");
                }

                // Добавляем временную метку создания
                var createdDate = DateTime.UtcNow;
                
                Order order = new()
                {
                    DeliveryLatitude = orderModel.DeliveryLatitude,
                    DeliveryLongitude = orderModel.DeliveryLongitude,
                    ProductId = orderModel.ProductId,
                    Quentity = orderModel.Quentity,
                    UserId = orderModel.UserId,
                    Status = OrderStatus.Created.ToString(),
                    CreatedBy = User.Identity.Name,
                    CreatedAt = createdDate, // Добавляем дату создания
                    UpdatedAt = createdDate
                };

                await _clientOrder.CreadOrdersAsync(order);

                _logger.LogMessage(User, $"Добавлена карточка товара в базу данных -- [{order.Id}]", LogLevel.Information);

                // Создаем объект ответа с нужными данными
                var response = new
                {
                    Success = true,
                    Message = "Order created successfully",
                    OrderId = order.Id,
                    CreatedDate = createdDate, // Возвращаем дату создания
                    Status = order.Status
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                
                // Возвращаем ошибку в структурированном виде
                var errorResponse = new
                {
                    Success = false,
                    Message = "An error occurred while creating the order",
                    Error = Auxiliary.GetDetailedExceptionMessage(ex)
                };
                
                return StatusCode(500, errorResponse);
            }
        }
        /// <summary>
        /// Подписка на статус заказа
        /// </summary>
        /// <param name="ordersId">Идентификатор заказа</param>
        /// <returns>Статус заказа в случае если значение изменилось</returns>
        [Authorize]
        [HttpGet("sseOrders/{ordersId}")]
        public async Task<IActionResult> StreamOrders(string ordersId)
        {
            try
            {
                Response.ContentType = "text/event-stream";
                Response.Headers.Append("Cache-Control", "no-cache");
                Response.Headers.Append("Connection", "keep-alive");

                string leastStatus = null;

                while (true)
                {
                    var currentStatus = await _clientOrder.GetOrderByIdAsync(Int32.Parse(ordersId));

                    if (currentStatus.Status != leastStatus)
                    {
                        leastStatus = currentStatus.Status;
                        await Response.WriteAsync($"Data: {currentStatus.Status} \n\n");
                        await Response.Body.FlushAsync();
                    }
                    await Task.Delay(10000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }

        /// <summary>
        /// Получение всех заказов
        /// </summary>
        /// <returns>Заказы</returns>
        [Authorize]
        [HttpGet("getOrders")]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _clientOrder.GetOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }

        /// <summary>
        /// Получение последних 5 заказов для текущего пользователя
        /// </summary>
        /// <returns>Список последних 5 заказов пользователя</returns>
        [Authorize]
        [HttpGet("getLastFiveOrders")]
        public async Task<IActionResult> GetLastFiveOrders()
        {
            try
            {
                var user = await _accountService.GetByNameUserAsync(User.Identity.Name);
                
                if (user == null)
                {
                    return Unauthorized("Пользователь не найден");
                }
                
                var lastFiveOrders = await _clientOrder.GetLastFiveOrdersByUserAsync(user);
                
                return Ok(lastFiveOrders);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return BadRequest(Auxiliary.GetDetailedExceptionMessage(ex));
            }
        }

        /// <summary>
        /// Удаление заказа по идентификатору
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <returns>Результат операции удаления</returns>
        [Authorize]
        [HttpDelete("deleteOrder/{orderId}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            try
            {
                // Опционально: проверка прав доступа
                var order = await _clientOrder.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound($"Заказ с ID {orderId} не найден");
                }

                // Проверка, что пользователь имеет право удалять этот заказ
                var currentUser = await _accountService.GetByNameUserAsync(User.Identity.Name);

                var result = await _clientOrder.DeleteOrderAsync(orderId);
                
                if (!result)
                {
                    return NotFound($"Заказ с ID {orderId} не найден или уже удален");
                }

                _logger.LogMessage(User, $"Удален заказ из базы данных -- [{orderId}]", LogLevel.Information);
                
                return Ok(new { success = true, message = "Заказ успешно удален" });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return BadRequest(new { success = false, error = Auxiliary.GetDetailedExceptionMessage(ex) });
            }
        }

        /// <summary>
        /// Удаление продукта по идентификатору
        /// </summary>
        /// <param name="productId">Идентификатор продукта</param>
        /// <returns>Результат операции удаления</returns>
        [Authorize]
        [HttpDelete("deleteProduct/{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                // Опционально: проверка прав доступа
                var product = await _clientOrder.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound($"Продукт с ID {productId} не найден");
                }

                // Проверка, что пользователь имеет право удалять этот продукт
                var currentUser = await _accountService.GetByNameUserAsync(User.Identity.Name);

                var result = await _clientOrder.DeleteProductAsync(productId);
                
                if (!result)
                {
                    return NotFound($"Продукт с ID {productId} не найден или уже удален");
                }

                _logger.LogMessage(User, $"Удален продукт из базы данных -- [{product.Name}]", LogLevel.Information);
                
                return Ok(new { success = true, message = "Продукт успешно удален" });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return BadRequest(new { success = false, error = Auxiliary.GetDetailedExceptionMessage(ex) });
            }
        }

    }
}
