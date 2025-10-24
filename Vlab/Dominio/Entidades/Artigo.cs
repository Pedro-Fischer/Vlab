using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.Entidades
{
    public class Artigo : Material
    {
        [Required]
        public string DOI { get; set; } = default!;


    }
}