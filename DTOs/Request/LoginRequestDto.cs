using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace cw3_apbd.DTOs.Request
{
    public class LoginRequestDto
    {   [Required(ErrorMessage = "Login nie może być pusty")]
        public string Login { get; set; }
        public string Haslo { get; set; }

    }
}
