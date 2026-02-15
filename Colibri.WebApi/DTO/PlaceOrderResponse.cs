using System;

namespace Colibri.WebApi.DTO;

public class PlaceOrderResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int OrderId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; }
}
