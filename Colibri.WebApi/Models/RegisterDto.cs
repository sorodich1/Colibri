using System.ComponentModel.DataAnnotations;

namespace Colibri.WebApi.Models
{
    /// <summary>
    /// Модель регистрации пользователя
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
        public string FirstName { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        [StringLength(50, ErrorMessage = "Фамилия не может быть длиннее 50 символов")]
        public string LastName { get; set; }
        /// <summary>
        /// Email пользователя
        /// </summary>
        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Роль пользователя
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        [Required(ErrorMessage = "Подтвердите пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
