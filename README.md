# Vlab — API

Este README documenta como configurar e usar a API Vlab, descreve as rotas principais, autenticação, exemplos de uso e as regras de negócio implementadas.

## Sumário

- Setup e execução
- Migrations
- Autenticação (JWT)
- Rotas principais (Usuários, Autores, Materiais)
- Exemplos de uso (curl / PowerShell)
- Regras de negócio
- Testes

---

## Setup e execução

Pré-requisitos
- .NET 9 SDK
- dotnet-ef (opcional, para migrações)

Passos para executar localmente (PowerShell):

```powershell
cd 'C:\Users\Pedro\OneDrive\Documentos\Vlab'
dotnet restore
dotnet build

# (opcional) criar/aplicar migrações no projeto que contém o DbContext
cd .\Vlab
# se necessário atualizar/install dotnet-ef: dotnet tool install --global dotnet-ef
dotnet ef database update

dotnet run --project .\Vlab\Vlab.csproj
```

Configuração importante
- A aplicação usa uma chave JWT em configuração (chave `Jwt` no `appsettings.json`). Se não houver, a aplicação usa o valor `Vlab` por padrão no código — para produção defina um valor forte em `appsettings.json` ou variáveis de ambiente.

---

## Migrations

- Crie migrações a partir do projeto `Vlab` (onde está o `DbContexto`) ou usando a factory de design time que já existe.
- Exemplo (a partir da pasta do projeto `Vlab`):

```powershell
dotnet ef migrations add NomeDaMigration --project . --startup-project ..\Vlab
dotnet ef database update --project . --startup-project ..\Vlab
```

Se o comando reclamar que já existe uma migration com o mesmo nome, use outro nome ou remova a migration duplicada com `dotnet ef migrations remove` (veja as advertências antes de remover se a migration já tiver sido aplicada no banco).

---

## Autenticação (JWT)

- Para obter um token JWT envie POST para `/api/usuarios/login` com JSON `{ "email": "...", "senha": "..." }`.
- O endpoint retorna um `TokenDTO` com `Token` e `DataExpiracao`.
- Para rotas protegidas (ex.: criação/atualização/exclusão de materiais) envie o header:

```
Authorization: Bearer <TOKEN_AQUI>
```

No código a aplicação lê a claim `NameIdentifier` do token para identificar o `userId` atual.

---

## Rotas principais

Observação: a API usa rotas prefixadas em `api/*`.

1) Usuários

- POST `/api/usuarios/registrar` — registra novo usuário.
  - Body: `UserCreateDTO` (Email, Senha, ...)
  - Respostas: 201, 400 (validação), 500

- POST `/api/usuarios/login` — realiza login e retorna token JWT.
  - Body: `LoginDTO` (Email, Senha)
  - Respostas: 200 com `TokenDTO`, 401, 400

2) Autores

- POST `/api/autores` — cria autor (pessoa ou instituição).
  - Body: `AutorDTO` (tipo depende do DTO do projeto)
  - Respostas: 201 com `AutorOutputDTO`, 400, 404

- GET `/api/autores/{id}` — busca autor por id.

3) Materiais

- POST `/api/materiais` — cria material (Livro/Artigo/Video).
  - Body: `MaterialDTO`.
  - Regras: dependendo de `TipoMaterial` campos específicos são obrigatórios (ver seção Regras de negócio).
  - Retorna 201 com `MaterialOutputDTO`.

- GET `/api/materiais` — lista com paginação e busca.
  - Query params: `pagina` (int), `query` (string), `status` (string)
  - Retorna `PagedList<MaterialOutputDTO>` com campos `Items`, `TotalCount`, `Page`.

- GET `/api/materiais/{id}` — busca por id. Retorna 403 se material não publicado e o usuário atual não é o criador.

- PUT `/api/materiais/{id}` — atualiza material (somente dono pode alterar); valida campos obrigatórios e formato de ISBN/DOI conforme o tipo.

- PATCH `/api/materiais/{id}/status` — atualiza status (`Rascunho`, `Publicado`). Somente o dono pode alterar; valida valor do status.

- DELETE `/api/materiais/{id}` — deleta material (somente dono).

---

## Exemplos de uso

1) Registrar usuário (PowerShell/curl):

```powershell
curl -Method POST -Uri http://localhost:5000/api/usuarios/registrar -Body (ConvertTo-Json @{ email = "user@example.com"; senha = "senha123" }) -ContentType "application/json"
```

2) Login e uso do token (PowerShell):

```powershell
#$login = @{ email = "user@example.com"; senha = "senha123" } | ConvertTo-Json
#$resp = Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/usuarios/login -Body $login -ContentType 'application/json'
$token = '<TOKEN_AQUI>'
Invoke-RestMethod -Method Get -Uri "http://localhost:5000/api/materiais?pagina=1" -Headers @{ Authorization = "Bearer $token" }
```

Exemplo curl equivalente:

```bash
curl -H "Authorization: Bearer <TOKEN>" "http://localhost:5000/api/materiais?pagina=1&query=termo"
```

3) Criar um livro:

```powershell
$body = @{ TipoMaterial = 'Livro'; Title = 'Meu Livro'; AutorId = 1; Status = 'Publicado'; ISBN = '9783161484100' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/materiais -Body $body -ContentType 'application/json' -Headers @{ Authorization = "Bearer $token" }
```

---

## Regras de negócio (resumo)

- Usuário
  - Email obrigatório e em formato válido.
  - Senha mínima de 6 caracteres (armazenada como hash BCrypt).
  - Email deve ser único.

- Autor
  - Existem 2 tipos via herança: `AutorPessoa` e `AutorInstituicao`.
  - AutorPessoa: `Nome` obrigatório (min 3, max 80); `DataNascimento` obrigatório, data válida e não pode ser futura.
  - AutorInstituicao: `Nome` obrigatório (min 3, max 120); `Cidade` obrigatório (min 2, max 80).

- Material
  - Campos obrigatórios: `TipoMaterial`, `Title`, `AutorId`, `Status`.
  - `Status` é um enum (ex.: `Rascunho`, `Publicado`); a busca aceita filtro por status.
  - Busca: faz filtro por `query` em título, descrição e nome do autor (case-insensitive).
  - Paginação: páginas de 10 itens; o serviço retorna `PagedList<T>` com `TotalCount` e `Page`.
  - Regras por tipo:
    - Livro: `ISBN` obrigatório; formato validado (ISBN-10/ISBN-13); tenta enriquecer dados via OpenLibrary (se configurado);
    - Artigo: `DOI` obrigatório; formato validado (regex tipo `10.xxxx/..`).
    - Video: `DuracaoMinutos` obrigatório e positivo.
  - Ações de atualização/alteração de status/deleção somente podem ser feitas pelo criador do material — caso contrário retorna 403/Forbid.

---

## Testes

- O repositório já contém um projeto de testes (`Vlab.Testes`).
- Executar testes:

```powershell
dotnet test "C:\Users\Pedro\OneDrive\Documentos\Vlab\Vlab.Testes\Vlab.Testes.csproj"
```

O projeto de testes possui testes de:
- validação de modelos (`Usuario`, `AutorPessoa`, `AutorInstituicao`),
- controller (ex.: `MaterialController` - filtros e respostas),
- smoke test que verifica que a assembly principal carrega.

---

## Observações e dicas

- Para desenvolvimento local use `ASPNETCORE_ENVIRONMENT=Development` para expor mensagens de erro completas.
- Se precisar adicionar testes de serviço que usam `DbContext`, prefira usar o provider `Microsoft.EntityFrameworkCore.InMemory` e construir o `DbContext` com `DbContextOptions<DbContexto>` em vez de instanciar a configuração via `IConfiguration` (isso facilita testes unitários). Se desejar eu posso adicionar exemplos de testes com InMemory.

Se quiser, eu posso:
- Gerar exemplos práticos de requisições para cada endpoint (ex.: payloads completos);
- Adicionar seção de arquitetura com diagrama simplificado;
- Gerar exemplos de testes para `MaterialServico` usando InMemory.

---

Obrigado — diga qual seção quer que eu detalhe mais (ex.: exemplos completos de payloads para cada tipo de material, ou instruções passo a passo para rodar migrações).
# Vlab