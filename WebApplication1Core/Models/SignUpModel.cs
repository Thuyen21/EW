﻿using System.ComponentModel.DataAnnotations;

namespace WebApplication1Core.Models
{
    public class SignUpModel
    {

        [Key]
        public string? id { get; set; }
        [Required]

        public string? Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]

        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]

        public string? Password { get; set; }
        [Required]
        public string? role { get; set; }
        [Required]

        public bool enable { get; set; }

    }
}