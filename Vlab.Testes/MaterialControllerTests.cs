using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Vlab.Controllers;
using Vlab.Dominio.DTOs;
using Vlab.Dominio.Interfaces;
using Vlab.Dominio.ModelViews;
using Xunit;

namespace Vlab.Testes
{
    public class MaterialControllerTests
    {
        [Fact]
        public async Task ListarMateriais_ForwardsFilterAndReturnsOk()
        {
            // arrange
            var mockService = new Mock<IMaterialServico>();
            var items = new List<MaterialOutputDTO>
            {
                new MaterialOutputDTO { Id = 1, Titulo = "A", Status = "Publicado" },
                new MaterialOutputDTO { Id = 2, Titulo = "B", Status = "Publicado" }
            };
            var paged = new PagedList<MaterialOutputDTO>(items, totalCount: 2, page: 2);

            MaterialFilter? capturedFilter = null;
            int? capturedUserId = null;

            mockService.Setup(s => s.Buscar(It.IsAny<MaterialFilter>(), It.IsAny<int?>()))
                .Callback<MaterialFilter, int?>((f, uid) => { capturedFilter = f; capturedUserId = uid; })
                .ReturnsAsync(paged);

            var controller = new MaterialController(mockService.Object);

            // set user id claim
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "42") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // act
            var result = await controller.ListarMateriais(new MaterialFilter(2, "term", "Publicado"));

            // assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(paged, ok.Value);
            Assert.NotNull(capturedFilter);
            Assert.Equal(2, capturedFilter!.pagina);
            Assert.Equal("term", capturedFilter.query);
            Assert.Equal("Publicado", capturedFilter.status);
            Assert.Equal(42, capturedUserId);
        }

        [Fact]
        public async Task BuscarPorId_WhenServiceThrowsUnauthorized_ReturnsForbid()
        {
            var mockService = new Mock<IMaterialServico>();
            mockService.Setup(s => s.BuscarPorId(It.IsAny<int>(), It.IsAny<int?>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            var controller = new MaterialController(mockService.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } };

            var result = await controller.BuscarPorId(1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task AtualizarStatus_WithNullNovoStatus_ReturnsBadRequest()
        {
            var mockService = new Mock<IMaterialServico>();
            var controller = new MaterialController(mockService.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } };

            var dto = new UpdateStatusDTO { NovoStatus = null };

            var result = await controller.AtualizarStatus(1, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("novo status", bad.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}
