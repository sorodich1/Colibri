using System;
using System.Collections.Generic;

namespace Colibri.WebApi.Models;

public class UserModel
{
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SurName { get; set; }
        public DateTime LastLogin { get; set; }
        public List<string> Roles { get; set; } = [];
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        
        public string FullName => $"{LastName} {FirstName} {SurName}".Trim();
        public string FormattedLastLogin => LastLogin.ToString("dd.MM.yyyy HH:mm");
        public bool IsLockedOut => LockoutEnabled && AccessFailedCount >= 5;
}
