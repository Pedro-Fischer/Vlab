using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vlab.Dominio.Entidades;

namespace Vlab.Infraestrutura.Db
{
    public class DbContexto : DbContext
    {
        private readonly IConfiguration _configuracaoAppSettings;
        public DbContexto(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }

        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Autor> Autores { get; set; } = default!;
        public DbSet<Material> Materiais { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Livro>()
                .HasIndex(b => b.ISBN)
                .IsUnique();

            modelBuilder.Entity<Artigo>()
                .HasIndex(a => a.DOI)
                .IsUnique();

            modelBuilder.Entity<Autor>()
                .HasDiscriminator<string>("TipoAutor")
                .HasValue<AutorPessoa>("Pessoa")
                .HasValue<AutorInstituicao>("Instituicao");

            modelBuilder.Entity<Material>()
                .HasDiscriminator<string>("TipoMaterial")
                .HasValue<Livro>("Livro")
                .HasValue<Artigo>("Artigo")
                .HasValue<Video>("Video");

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Autor);

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Criador);
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("MySql")?.ToString();
                if (!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseMySql(stringConexao, ServerVersion.AutoDetect(stringConexao));
                }
            }
        }
    }
}