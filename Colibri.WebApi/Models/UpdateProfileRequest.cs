using System;

namespace Colibri.WebApi.Models;

public class UpdateProfileRequest
{
    /// <summary>
    /// 
    /// </summary>
    public string UserName { get; set; }
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; set; }
        
    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName { get; set; }
    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; }
}
