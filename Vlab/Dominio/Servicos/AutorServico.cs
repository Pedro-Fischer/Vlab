using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.Entidades;
using Vlab.Dominio.Interfaces;
using Vlab.Infraestrutura.Db;

namespace Vlab.Dominio.Servicos
{
    public class AutorServico : IAutorServico
    {

        private readonly DbContexto _contexto;

        public AutorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<AutorOutputDTO> Criar(AutorDTO dto)
        {
            var novoAutor = ValidarEMapearAutor(dto);

            _contexto.Autores.Add(novoAutor);
            await _contexto.SaveChangesAsync();

            return MapearParaOutputDTO(novoAutor);
        }

        public async Task<AutorOutputDTO> BuscarPorId(int id)
        {
            var autor = await _contexto.Autores.Where(a => a.Id == id).FirstOrDefaultAsync();

            return MapearParaOutputDTO(autor!);
        }

        private Autor ValidarEMapearAutor(AutorDTO dto, int? id = null)
        {
            if (string.IsNullOrEmpty(dto.TipoAutor))
            {
                 throw new ValidationException("O campo TipoAutor é obrigatório.");
            }

            if (dto.TipoAutor.Equals("Pessoa", StringComparison.OrdinalIgnoreCase))
            {
                if (!dto.DataNascimento.HasValue || dto.DataNascimento > DateTime.Now)
                {
                     throw new ValidationException("Data de Nascimento é obrigatória e não pode ser futura para Autores Pessoa.");
                }
                
                return new Autor
                { 
                    Id = id ?? 0,
                    Nome = dto.Nome,
                    DataNascimento = dto.DataNascimento.Value
                };
            }
            else if (dto.TipoAutor.Equals("Instituicao", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(dto.Cidade))
                {
                     throw new ValidationException("Cidade é obrigatória para Autores Instituição.");
                }
                
                return new Autor
                { 
                    Id = id ?? 0,
                    Nome = dto.Nome, 
                    Cidade = dto.Cidade 
                };
            }
            else 
            {
                throw new ArgumentException($"Tipo de Autor '{dto.TipoAutor}' inválido. Use 'Pessoa' ou 'Instituicao'.");
            }
        }
        
        private AutorOutputDTO MapearParaOutputDTO(Autor autor)
        {
            var dto = new AutorOutputDTO
            {
                Id = autor.Id,
                Nome = autor.Nome,
                TipoAutor = autor.GetType().Name
            };

            if (autor is AutorPessoa pessoa)
            {
                dto.DataNascimento = pessoa.DataNascimento;
                dto.Cidade = null;
            }
            else if (autor is AutorInstituicao instituicao)
            {
                dto.Cidade = instituicao.Cidade;
                dto.DataNascimento = null;
            }
            
            return dto;
        }
        
    }
}