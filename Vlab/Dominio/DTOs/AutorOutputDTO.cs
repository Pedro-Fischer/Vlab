using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.DTOs
{
    public class AutorOutputDTO
    {
        public int Id { get; set; }
        public string TipoAutor { get; set; } = default!;
        public string Nome { get; set; } = default!;

        public DateTime? DataNascimento { get; set; } 

        public string? Cidade { get; set; } 
    }
}
    