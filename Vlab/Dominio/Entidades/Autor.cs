using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.Entidades
{
    public class Autor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string TipoAutor { get; set; } = default!;

        public string Nome { get; set; } = default!;
        
        public DateTime? DataNascimento { get; set; } 
        
        public string? Cidade { get; set; }
    }
}