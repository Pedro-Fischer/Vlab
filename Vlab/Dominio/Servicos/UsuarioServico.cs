using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.Entidades;
using Vlab.Dominio.Interfaces;
using Vlab.Infraestrutura.Db;

namespace Vlab.Dominio.Servicos
{
    public class UsuarioServico : IUsuarioServico
    {
        private readonly DbContexto _contexto;
        private readonly IConfiguration _configuracao;

        public UsuarioServico(DbContexto contexto, IConfiguration configuracao)
        {
            _contexto = contexto;
            _configuracao = configuracao;
        }

        public async Task<Usuario?> BuscarPorId(int id)
        {
            return await _contexto.Usuarios.FindAsync(id);
        }

        public async Task<TokenDTO> Login(LoginDTO dto)
        {
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
            {
                throw new ArgumentException("Credenciais inválidas.");
            }

            var senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha);

            if (!senhaCorreta)
            {
                throw new ArgumentException("Senha incorreta.");
            }

            return GerarToken(usuario);
        }
        private TokenDTO GerarToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            
            var jwtKey = _configuracao.GetValue<string>("Jwt") ?? "Vlab";  
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), 
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return new TokenDTO 
            { 
                Token = tokenHandler.WriteToken(token), 
                DataExpiracao = tokenDescriptor.Expires.Value 
            };
        }






        public async Task<UserOutputDTO> Cadastrar(UserCreateDTO userCreateDTO)
        {
            var existeUsuario = await _contexto.Usuarios.AnyAsync(u => u.Email == userCreateDTO.Email);
            if (existeUsuario)
            {
                throw new ArgumentException("E-mail já cadastrado.");
            }

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(userCreateDTO.Senha);
            
            var novoUsuario = new Usuario
            {
                Email = userCreateDTO.Email,
                Senha = senhaHash,
            };

            _contexto.Usuarios.Add(novoUsuario);
            await _contexto.SaveChangesAsync();

            return new UserOutputDTO
            {
                Id = novoUsuario.Id,
                Email = novoUsuario.Email,
            };
        }
    }
}