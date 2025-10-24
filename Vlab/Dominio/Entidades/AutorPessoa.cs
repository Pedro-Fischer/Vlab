using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vlab.Dominio.Entidades
{
    public class AutorPessoa : Autor
    {
        [Required]
        [MinLength(3)]  
        [MaxLength(80)]
        public string Name { get; set; } = default!;

        [Required]
        [DataType(DataType.Date)]
        [PastOrPresent(ErrorMessage = "Data de nascimento não pode ser no futuro.")]
        public DateTime BirthDate { get; set; } = default!;

        public class PastOrPresentAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            if (value is DateTime dt) return dt.Date <= DateTime.Today;
            return false;
        }

        public override string FormatErrorMessage(string name) =>
            string.IsNullOrEmpty(ErrorMessage) ? $"{name} não pode ser no futuro." : ErrorMessage;
    }

    }
}