using System.ComponentModel.DataAnnotations;

namespace Colibri.WebApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
