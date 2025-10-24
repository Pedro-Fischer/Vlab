using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vlab.Dominio.Enuns;

namespace Vlab.Dominio.DTOs
{
    public class MaterialDTO
    {
        
        public string TipoMaterial { get; set; } = default!;
        public string Title { get; set; } = default!; 
        public string? Description { get; set; } 
        public int AutorId { get; set; }
        public string Status { get; set; } = default!; 
        
        public string? ISBN { get; set; }
        public int? NumeroPaginas { get; set; }

        public string? DOI { get; set; }

        public int? DuracaoMinutos { get; set; }

    }
    
}