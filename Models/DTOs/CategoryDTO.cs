using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models.DTOs
{
    public class CategoryDTO
    {
        [Required]
        public string? CategoryName { get; set; }
    }
}
