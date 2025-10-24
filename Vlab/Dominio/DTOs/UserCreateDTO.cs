using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.DTOs
{
    public class UserCreateDTO
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
    }
}