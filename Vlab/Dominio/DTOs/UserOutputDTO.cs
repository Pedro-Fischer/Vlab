using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.DTOs
{
    public class UserOutputDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
    }
}