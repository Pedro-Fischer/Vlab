using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.DTOs
{
    public class TokenDTO
    {
        public string Token { get; set; } = default!;
        public DateTime DataExpiracao { get; set; }
    }
}