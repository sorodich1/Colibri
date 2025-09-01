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
                    return BadRequest();
                }

                Order order = new()
                {
                    DeliveryLatitude = orderModel.DeliveryLatitude,
                    DeliveryLongitude = orderModel.DeliveryLongitude,
                    ProductId = orderModel.ProductId,
                    Quentity = orderModel.Quentity,
                    UserId = orderModel.UserId,
                    Status = OrderStatus.Created.ToString()
                };


                await _clientOrder.CreadOrdersAsync(order);

                _logger.LogMessage(User, $"Добавлена карточка товара в базу данных -- [{order.Id}]", LogLevel.Information);

                return Ok("succes");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok(Auxiliary.GetDetailedExceptionMessage(ex));
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
    }
}
