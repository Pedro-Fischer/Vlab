using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.Interfaces;

namespace Vlab.Controllers
{
    [ApiController]
    [Route("api/autores")]
    public class AutorController : ControllerBase
    {
        private readonly IAutorServico _autorServico;

        public AutorController(IAutorServico autorServico)
        {
            _autorServico = autorServico;
        }

        private int GetCurrentUserId()
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return 1;
            return int.Parse(userIdClaim);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AutorOutputDTO), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CriarAutor([FromBody] AutorDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var creatorUserId = GetCurrentUserId();

                var autorCriado = await _autorServico.Criar(dto);

                // Retorna 201 Created (HTTP)
                return CreatedAtAction(
                    nameof(BuscarPorId),
                    new { id = autorCriado.Id },
                    autorCriado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AutorOutputDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            try
            {
                var autor = await _autorServico.BuscarPorId(id);

                return Ok(autor);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

    }
}