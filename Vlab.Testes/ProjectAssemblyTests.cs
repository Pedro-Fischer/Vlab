using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Vlab.Testes
{
    public class ProjectAssemblyTests
    {
        [Fact]
        public void ProjectAssembly_IsLoadable_AndContainsTypes()
        {
            // tenta encontrar assembly já carregada
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name.Equals("Vlab", StringComparison.OrdinalIgnoreCase));

            // se não estiver carregada, tenta carregar do diretório de saída
            if (asm == null)
            {
                var candidate = Path.Combine(AppContext.BaseDirectory, "Vlab.dll");
                Assert.True(File.Exists(candidate), $"Não encontrou {candidate}. Compile o projeto antes.");
                asm = Assembly.LoadFrom(candidate);
            }

            Assert.NotNull(asm);
            var types = asm.GetTypes();
            Assert.True(types.Length > 0, "Assembly carregado, mas sem tipos públicos.");
        }
    }
}
