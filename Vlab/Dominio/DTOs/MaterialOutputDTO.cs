using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.DTOs
{
    public class MaterialOutputDTO
    {
        public int Id { get; set; } 
        public string TipoMaterial { get; set; } = default!; 

        public AutorOutputDTO? Autor { get; set; } 
        
        public UserOutputDTO? Criador { get; set; } 

        public string Titulo { get; set; } = default!;
        public string Status { get; set; } = default!; 
        public string? Descricao { get; set; } 
        
        public string? ISBN { get; set; }
        public int? NumeroPaginas { get; set; } 

        // 2. Artigo
        public string? DOI { get; set; }

        // 3. Vídeo
        public int? DuracaoMinutos { get; set; }
    }
}