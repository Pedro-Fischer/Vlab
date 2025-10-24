using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.ModelViews;

namespace Vlab.Dominio.Interfaces
{
    public record MaterialFilter(int pagina, string? query, string? status);

    public interface IMaterialServico
    {
        
        Task<MaterialOutputDTO> Criar(MaterialDTO dto, int creatorUserId);
        
        Task<PagedList<MaterialOutputDTO>> Buscar(MaterialFilter filter, int? userId);

        Task<MaterialOutputDTO> BuscarPorId(int id, int? userId);

        Task<MaterialOutputDTO> Atualizar(int id, MaterialDTO dto, int userId);

        Task<MaterialOutputDTO> AtualizarStatus(int id, string novoStatus, int userId);

        Task Deletar(int id, int userId);
    }
    
}