using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.Entidades
{
    public class Livro : Material
    {
        [Required]
        
        public string ISBN { get; set; } = default!;

        [Required]
        [MinLength(1)]
        public int NumeroDePaginas { get; set; }
    }
}