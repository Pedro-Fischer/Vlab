using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.Entidades;
using Vlab.Dominio.Enuns;
using Vlab.Dominio.Interfaces;
using Vlab.Dominio.ModelViews;
using Vlab.Infraestrutura.Db;

namespace Vlab.Dominio.Servicos
{
    public class MaterialServico : IMaterialServico
    {
        private readonly DbContexto _contexto;
        private readonly IAutorServico _autorServico;
        private readonly IOpenLibraryServico _openLibraryService; 
        private readonly IUsuarioServico _usuarioServico;

        public MaterialServico(DbContexto contexto, IAutorServico autorServico, IOpenLibraryServico openLibraryService, IUsuarioServico usuarioServico)
        {
            _contexto = contexto;
            _autorServico = autorServico;
            _openLibraryService = openLibraryService;
            _usuarioServico = usuarioServico;
        }

        public async Task<MaterialOutputDTO> Criar(MaterialDTO dto, int creatorUserId)
        {
            ValidarCamposObrigatorios(dto);
            await _autorServico.BuscarPorId(dto.AutorId);

            var novoMaterial = await ValidarEMapearMaterial(dto, creatorUserId);

            _contexto.Materiais.Add(novoMaterial);
            await _contexto.SaveChangesAsync();

            return await MapearParaOutputDTO(novoMaterial);
        }

        public async Task<PagedList<MaterialOutputDTO>> Buscar(MaterialFilter filter, int? userId)
        {
            var query = _contexto.Materiais
                .Include(m => m.Autor)
                .Include(m => m.Criador)
                .AsQueryable();
        

            if (!string.IsNullOrEmpty(filter.status) && Enum.TryParse(filter.status, true, out StatusMaterial statusEnum))
            {
                query = query.Where(m => m.Status == statusEnum);
            }

            // FILTRO DE BUSCA (Título, Descrição, Autor - Requisito 2.6)
            if (!string.IsNullOrEmpty(filter.query))
            {
                var termo = filter.query.ToLower().Trim();
                query = query.Where(m =>
                    m.Titulo.ToLower().Contains(termo) ||
                    (m.Descricao != null && m.Descricao.ToLower().Contains(termo)) ||
                    (m.Autor != null && m.Autor.Nome.ToLower().Contains(termo))
                );
            }

            // PAGINAÇÃO
            var totalCount = await query.CountAsync();
            int itensPorPagina = 10;
            var items = await query
                .Skip((filter.pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            var dtosTasks = items.Select(MapearParaOutputDTO);
            var dtos = await Task.WhenAll(dtosTasks);

            return new PagedList<MaterialOutputDTO>(dtos.ToList(), totalCount, filter.pagina);
        }

        public async Task<MaterialOutputDTO> BuscarPorId(int id, int? userId)
        {
            var material = await ObterEntidadePorId(id);

            if (material.Status != StatusMaterial.Publicado && 
                (!userId.HasValue || material.CriadorId != userId.Value))
            {
                throw new UnauthorizedAccessException("Acesso negado. Material não publicado.");
            }

            return await MapearParaOutputDTO(material);
        }

        public async Task<MaterialOutputDTO> Atualizar(int id, MaterialDTO dto, int userId)
        {
            var materialExistente = await ObterEntidadePorId(id);
            ChecarPosse(materialExistente, userId);

            ValidarCamposObrigatorios(dto);
            if (dto.AutorId != materialExistente.AutorId)
            {
                await _autorServico.BuscarPorId(dto.AutorId);
            }

            materialExistente.AutorId = dto.AutorId;
            materialExistente.Titulo = dto.Title;
            materialExistente.Descricao = dto.Description;
            materialExistente.Status = materialExistente.Status;
            
            if (materialExistente.TipoMaterial == "Livro" && materialExistente is Livro livro)
            {
                livro.ISBN = dto.ISBN ?? livro.ISBN;
            }
            if (materialExistente.TipoMaterial == "Artigo" && materialExistente is Artigo artigo)
            {
                artigo.DOI = dto.DOI ?? artigo.DOI;
            }
            if (materialExistente.TipoMaterial == "Video" && materialExistente is Video video)
            {
                video.Duracao = dto.DuracaoMinutos ?? video.Duracao;
            }

            await _contexto.SaveChangesAsync();
            return await MapearParaOutputDTO(materialExistente);
        }

        public async Task<MaterialOutputDTO> AtualizarStatus(int id, string novoStatus, int userId)
        {
            var material = await ObterEntidadePorId(id);
            ChecarPosse(material, userId);

            if (!Enum.TryParse(novoStatus, true, out StatusMaterial novoStatusEnum))
            {
                throw new ArgumentException($"Status '{novoStatus}' inválido. Use Rascunho ou Publicado.");
            }

            material.Status = novoStatusEnum;

            await _contexto.SaveChangesAsync();
            return await MapearParaOutputDTO(material);
        }
        
        public async Task Deletar(int id, int userId)
        {
            var material = await ObterEntidadePorId(id);
            ChecarPosse(material, userId);

            _contexto.Materiais.Remove(material);
            await _contexto.SaveChangesAsync();
        }

        private void ChecarPosse(Material material, int userId)
        {
            if (material.Criador.Id != userId)
            {
                throw new UnauthorizedAccessException("Acesso negado. Você só pode alterar/remover materiais que você mesmo cadastrou.");
            }
        }

        private async Task<Material> ObterEntidadePorId(int id)
        {
            var material = await _contexto.Materiais
                .Include(m => m.Autor)
                .Include(m => m.Criador)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null)
            {
                throw new KeyNotFoundException($"Material com ID {id} não encontrado.");
            }
            return material;
        }

        private void ValidarCamposObrigatorios(MaterialDTO dto)
        {
            if (string.IsNullOrEmpty(dto.TipoMaterial) || string.IsNullOrEmpty(dto.Title) || dto.AutorId <= 0 || string.IsNullOrEmpty(dto.Status))
            {
                throw new ArgumentException("Os campos TipoMaterial, Title, AutorId e Status são obrigatórios e devem ser válidos.");
            }
            if (!Enum.TryParse(dto.Status, true, out StatusMaterial _))
            {
                throw new ArgumentException($"Status '{dto.Status}' inválido. Use Rascunho, Publicado, ou outro valor válido.");
            }
        }

        private async Task<Material> ValidarEMapearMaterial(MaterialDTO dto, int creatorUserId, int? id = null)
        {
            var tipo = dto.TipoMaterial.ToLower();

            var material = new Material
            {
                Id = id ?? 0,
                Titulo = dto.Title,
                Descricao = dto.Description,
                AutorId = dto.AutorId,
                Status = Enum.Parse<StatusMaterial>(dto.Status, true),
                CriadorId = creatorUserId,
            };

            // LÓGICA CONDICIONAL E HERANÇA
            if (tipo == "livro")
            {
                if (string.IsNullOrEmpty(dto.ISBN))
                {
                    throw new ArgumentException("ISBN é obrigatório para materiais do tipo Livro.");
                }

                // Validação de Formato
                ValidarFormatoIsbn(dto.ISBN!);

                // Integração API Externa (Requisito 2.5)
                var bookData = await _openLibraryService.GetBookDataByIsbn(dto.ISBN!);

                var livro = new Livro
                {
                    ISBN = dto.ISBN!,
                    // Usa a propriedade da Entidade: NumeroDePaginas
                    // Valor: DTO > API > 0
                    NumeroDePaginas = dto.NumeroPaginas ?? bookData?.NumeroPaginas ?? 0
                };

                _contexto.Entry(livro).CurrentValues.SetValues(material);
                return livro;
            }
            else if (tipo == "artigo")
            {
                if (string.IsNullOrEmpty(dto.DOI))
                {
                    throw new ArgumentException("DOI é obrigatório para materiais do tipo Artigo.");
                }

                // Validação de Formato
                ValidarFormatoDoi(dto.DOI!);

                var artigo = new Artigo
                {
                    DOI = dto.DOI!
                };
                _contexto.Entry(artigo).CurrentValues.SetValues(material);
                return artigo;
            }
            else if (tipo == "video")
            {
                if (!dto.DuracaoMinutos.HasValue || dto.DuracaoMinutos <= 0)
                {
                    throw new ArgumentException("DuracaoMinutos é obrigatório e deve ser positivo para Vídeos.");
                }

                var video = new Video
                {
                    Duracao = dto.DuracaoMinutos.Value
                };
                _contexto.Entry(video).CurrentValues.SetValues(material);
                return video;
            }
            else
            {
                throw new ArgumentException($"Tipo de Material '{dto.TipoMaterial}' inválido. Use 'Livro', 'Artigo' ou 'Video'.");
            }
        }

        private void ValidarFormatoIsbn(string isbn)
        {
            // Regex para ISBN-10 ou ISBN-13
            if (!Regex.IsMatch(isbn, @"^(\d{9}[\dXx]|\d{13})$"))
            {
                throw new ArgumentException("O formato do ISBN está inválido. Use ISBN-10 ou ISBN-13 válido.");
            }
        }

        private void ValidarFormatoDoi(string doi)
        {
            // Regex para DOI (padrão '10.xxxx/yyyy')
            if (!Regex.IsMatch(doi, @"^10\.\d{4,9}\/[-._;()/:a-zA-Z0-9]+$"))
            {
                throw new ArgumentException("O formato do DOI está inválido. Deve ser um DOI válido (ex: 10.1234/abc.5678).");
            }
        }

        private async Task<MaterialOutputDTO> MapearParaOutputDTO(Material material)
        {
            var userEntity = await _usuarioServico.BuscarPorId(material.CriadorId);
            var userDto = new UserOutputDTO 
            { 
                Id = userEntity?.Id ?? 0, 
                Email = userEntity?.Email ?? "Desconhecido"
            };

            // Mapeamento do Autor
            var autorDto = material.Autor != null 
                ? new AutorOutputDTO
                {
                    Id = material.Autor.Id,
                    Nome = material.Autor.Nome,
                    TipoAutor = material.Autor.GetType().Name,

                    DataNascimento = (material.Autor as AutorPessoa)?.DataNascimento,
                    Cidade = (material.Autor as AutorInstituicao)?.Cidade,
                }
                : null;
            
            var dto = new MaterialOutputDTO
            {
                Id = material.Id,
                Titulo = material.Titulo,
                Descricao = material.Descricao,
                Status = material.Status.ToString(),
                TipoMaterial = material.GetType().Name.Replace("Material", ""),
                
                Autor = autorDto,
                Criador = userDto,

                // Mapeamento Condicional dos Campos Específicos
                ISBN = (material as Livro)?.ISBN,
                NumeroPaginas = (material as Livro)?.NumeroDePaginas,
                
                DOI = (material as Artigo)?.DOI,
                
                DuracaoMinutos = (material as Video)?.Duracao,
            };
            
            return dto;
        }

        
    }
}