using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vlab.Dominio.Entidades;
using Xunit;

namespace Vlab.Testes
{
    public class AutorValidationTests
    {
        [Fact]
        public void AutorPessoa_FutureBirthDate_ShouldFailValidation()
        {
            var autor = new AutorPessoa
            {
                Name = "Fulano",
                BirthDate = DateTime.Today.AddDays(1)
            };

            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(autor);
            var isValid = Validator.TryValidateObject(autor, ctx, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("não pode"));
        }

        [Fact]
        public void AutorInstituicao_ShortCity_ShouldFailValidation()
        {
            var autor = new AutorInstituicao
            {
                Name = "Uni",
                City = "A"
            };

            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(autor);
            var isValid = Validator.TryValidateObject(autor, ctx, results, true);

            Assert.False(isValid);
        }

        [Fact]
        public void AutorInstituicao_ValidData_ShouldPassValidation()
        {
            var autor = new AutorInstituicao
            {
                Name = "Universidade X",
                City = "São Paulo"
            };

            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(autor);
            var isValid = Validator.TryValidateObject(autor, ctx, results, true);

            Assert.True(isValid, string.Join("; ", results));
        }
    }
}
