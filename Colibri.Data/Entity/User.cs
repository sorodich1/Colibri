using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Colibri.Data.Entity
{
    /// <summary>
    /// Сущность пользователя
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// Имя
        /// </summary>
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = "NoName";
        /// <summary>
        /// Фамилия
        /// </summary>
        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = "NoName";
        /// <summary>
        /// Отчество
        /// </summary>
        [Required]
        [Display(Name = "Отчество")]
        public string SurName { get; set; } = "NoName";
        /// <summary>
        /// Дата создания юзера
        /// </summary>
        [Required]
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Email пользователя
        /// </summary>
        public override string Email { get; set; } = null;
    }
}
