using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.Entidades
{
    public class AutorInstituicao : Autor
    {
        [Required]
        [MinLength(3)]  
        [MaxLength(120)]
        public string Name { get; set; } = default!;

        [Required]
        [MinLength(2)]  
        [MaxLength(80)]
        public string City { get; set; } = default!;
    }
}