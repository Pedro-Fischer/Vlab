using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.Interfaces;
using Vlab.Dominio.ModelViews;

namespace Vlab.Controllers
{
    [ApiController]
    [Route("api/materiais")]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialServico _materialServico;

        public MaterialController(IMaterialServico materialServico)
        {
            _materialServico = materialServico;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim)) return 1;

            return int.Parse(userIdClaim);
        }

        [HttpPost]
        [ProducesResponseType(typeof(MaterialOutputDTO), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CriarMaterial([FromBody] MaterialDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var creatorUserId = GetCurrentUserId();
                var resultado = await _materialServico.Criar(dto, creatorUserId);

                return CreatedAtAction(nameof(BuscarPorId), new { id = resultado.Id }, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedList<MaterialOutputDTO>), 200)]
        public async Task<IActionResult> ListarMateriais([FromQuery] MaterialFilter filter)
        {
            var userId = GetCurrentUserId();

            var pagedList = await _materialServico.Buscar(filter, userId);

            return Ok(pagedList);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MaterialOutputDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var material = await _materialServico.BuscarPorId(id, userId);
                return Ok(material);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Material com ID {id} não encontrado." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); 
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MaterialOutputDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> AtualizarMaterial(int id, [FromBody] MaterialDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                var materialAtualizado = await _materialServico.Atualizar(id, dto, userId);

                return Ok(materialAtualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(MaterialOutputDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> AtualizarStatus(int id, [FromBody] UpdateStatusDTO dto)
        {
            if (string.IsNullOrEmpty(dto?.NovoStatus))
            {
                return BadRequest(new { message = "O novo status é obrigatório." });
            }

            try
            {
                var userId = GetCurrentUserId();
                var materialAtualizado = await _materialServico.AtualizarStatus(id, dto.NovoStatus, userId);

                return Ok(materialAtualizado);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DeletarMaterial(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _materialServico.Deletar(id, userId);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

    }
    
    public class UpdateStatusDTO
    {
        public string? NovoStatus { get; set; }
    }
}