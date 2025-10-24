using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vlab.Dominio.DTOs.Externos;

namespace Vlab.Dominio.Interfaces
{
    public interface IOpenLibraryServico
    {
        /// <summary>
        /// Busca dados de um livro na Open Library usando o ISBN.
        /// </summary>
        /// <param name="isbn">O ISBN do livro.</param>
        /// <returns>Um DTO com os dados do livro ou null se não for encontrado.</returns>
        Task<BookDataDTO?> GetBookDataByIsbn(string isbn);
    }
}