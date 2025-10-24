using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vlab.Dominio.DTOs.Externos;
using Vlab.Dominio.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Vlab.Dominio.Servicos.Externos
{
    public class OpenLibraryService : IOpenLibraryServico
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenLibraryService> _logger;

        public OpenLibraryService(HttpClient httpClient, ILogger<OpenLibraryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<BookDataDTO?> GetBookDataByIsbn(string isbn)
        {
            var url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Lança exceção para 4xx/5xx

                var content = await response.Content.ReadAsStringAsync();

                // A resposta da Open Library é complexa (JSON aninhado e dinâmico)
                var jsonDocument = JsonDocument.Parse(content);

                // O nó principal é dinâmico (ex: {"ISBN:9780321765723": {...}})
                var rootProperty = jsonDocument.RootElement.EnumerateObject().FirstOrDefault();

                if (rootProperty.Value.ValueKind == JsonValueKind.Undefined)
                {
                    _logger.LogWarning($"ISBN {isbn} não encontrado na Open Library.");
                    return null;
                }

                var bookElement = rootProperty.Value;

                // Busca o número de páginas
                var pages = bookElement.TryGetProperty("number_of_pages", out var pagesElement) &&
                            pagesElement.ValueKind == JsonValueKind.Number
                            ? pagesElement.GetInt32() : (int?)null;

                // Busca o título
                var title = bookElement.TryGetProperty("title", out var titleElement) &&
                            titleElement.ValueKind == JsonValueKind.String
                            ? titleElement.GetString() : null;

                return new BookDataDTO
                {
                    Titulo = title,
                    NumeroPaginas = pages
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Erro ao acessar Open Library para ISBN {isbn}: {ex.Message}");
                return null; // Retorna null em caso de falha na requisição
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro durante o processamento da resposta da Open Library: {ex.Message}");
                return null;
            }
        }
    }
}