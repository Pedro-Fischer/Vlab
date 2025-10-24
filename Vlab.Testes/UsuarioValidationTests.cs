using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vlab.Dominio.Entidades;
using Xunit;

namespace Vlab.Testes
{
    public class UsuarioValidationTests
    {
        [Fact]
        public void Usuario_InvalidEmail_ShouldFailValidation()
        {
            var usuario = new Usuario
            {
                Email = "invalido",
                Senha = "abcdef"
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(usuario);
            var isValid = Validator.TryValidateObject(usuario, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Email"));
        }

        [Fact]
        public void Usuario_ShortPassword_ShouldFailValidation()
        {
            var usuario = new Usuario
            {
                Email = "teste@x.com",
                Senha = "123"
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(usuario);
            var isValid = Validator.TryValidateObject(usuario, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("senha", System.StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Usuario_ValidData_ShouldPassValidation()
        {
            var usuario = new Usuario
            {
                Email = "teste@x.com",
                Senha = "123456"
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(usuario);
            var isValid = Validator.TryValidateObject(usuario, context, results, validateAllProperties: true);

            Assert.True(isValid, string.Join("; ", results));
        }
    }
}
