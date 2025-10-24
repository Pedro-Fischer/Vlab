using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Vlab.Dominio.Enuns;

namespace Vlab.Dominio.Entidades
{
    public class Material
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Titulo { get; set; } = default!;

        [MaxLength(1000)]
        public string? Descricao { get; set; } = default!;

        [Required]
        public int AutorId { get; set; } 

        [ForeignKey(nameof(AutorId))]
        public Autor Autor { get; set; } = default!;

        [Required]
        public StatusMaterial Status { get; set; }
        
        [Required]
        public int CriadorId { get; set; }

        [ForeignKey(nameof(CriadorId))]
        public Usuario Criador { get; set; } = default!;

        [Required]
        public string TipoMaterial { get; set; } = default!;

    }
}