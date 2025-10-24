using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.Entidades;

namespace Vlab.Dominio.Interfaces
{
    public interface IAutorServico
    {
        Task<AutorOutputDTO> Criar(AutorDTO dto);

        Task<AutorOutputDTO> BuscarPorId(int id);

    }
}