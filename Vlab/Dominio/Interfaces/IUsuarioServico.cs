using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vlab.Dominio.Entidades;
using Vlab.Dominio.DTOs;

namespace Vlab.Dominio.Interfaces
{
    public interface IUsuarioServico
    {
        Task<TokenDTO> Login(LoginDTO dto);
        Task<Usuario?> BuscarPorId(int id);
        Task<UserOutputDTO> Cadastrar(UserCreateDTO userCreateDTO);
    }
}