using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;

namespace Backend.Models.Authentication
{
    public class LoginModel
    {
        [Required]
        public string? UsernameOrEmail { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        // [Required]
        // public string? IdToken { get; set; }
    }
}
