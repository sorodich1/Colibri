using Colibri.GetDirection.Helpers;

using Newtonsoft.Json;

namespace Colibri.WebApi.Models
{
    /// <summary>
    /// Позиция покупателя и продовца
    /// </summary>
    public class OrderLocation
    {
        /// <summary>
        /// Широта
        /// </summary>
        [JsonProperty("sellerPoint")]
        public Point SellerPoint { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("buyerPoint ")]
        public Point BuyerPoint { get; set; }
    }
}
