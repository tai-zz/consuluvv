# Sistema de Gestão de Consultas UVV

Aplicação web em **ASP.NET Core 8 (MVC)** para gerenciamento de usuários e registro de
consultas médicas/profissionais. Projeto da disciplina **Desenvolvimento Web Back-end**.

## Integrantes do grupo

| Nome | Matrícula |
|----------|--------|
| Brunno Gaiba Bresaola Barbosa | 202531291 |
| Iuri Gravel do Nascimento | 202530765 |
| João Eduardo Pinto | 202529771 |

## Tecnologias

- ASP.NET Core 8 — MVC (Controllers + Views Razor) + View Component
- Entity Framework Core 8 (abordagem **Code First** + **Migrations**)
- SQL Server (instância `.\SQLEXPRESS` por padrão)
- Autenticação por **cookie** (customizada, com hash de senha via `PasswordHasher`)
- `BackgroundService` (serviço em segundo plano) para os lembretes de consulta
- Validação client-side (jQuery Unobtrusive Validation) + tema próprio (CSS, sem Bootstrap)

## Arquitetura (Separação de Preocupações)

```
ConsultasUVV/
├── Controllers/         # Home, Conta, Consultas, Notificacoes
├── Models/              # Usuario, Consulta, Notificacao (entidades Code First)
│   └── ViewModels/      # Login, Registro, EsqueciSenha, RedefinirSenha
├── Data/                # AppDbContext (registrado via DI no Program.cs)
├── Services/            # LembreteConsultaService (BackgroundService)
├── ViewComponents/      # NotificacoesBadge (contador do sino)
├── Validation/          # CpfAttribute (Data Annotation customizada)
├── Migrations/          # InitialCreate, AddCpfResetSenhaNotificacoes + ModelSnapshot
├── Views/               # Home, Conta, Consultas, Notificacoes, Shared
├── wwwroot/             # css/site.css
├── Program.cs           # DI + pipeline de middleware + AddHostedService
└── appsettings.json     # Connection String
```

### Entidades

| Entidade | Campos |
|----------|--------|
| `Usuario` | `Nome`, `Email` (**único**), `Cpf` (**único**), `Senha` (hash), `DataCadastro`, `TokenResetSenha`, `TokenResetExpiraEm` |
| `Consulta` | `Especialidade`, `DataHora`, `Descricao`, `UsuarioId` (FK → Usuario) |
| `Notificacao` | `Mensagem`, `CriadaEm`, `Lida`, `UsuarioId` (FK), `ConsultaId` (FK opcional) |

Relacionamentos **1:N**: um usuário tem várias consultas e várias notificações
(exclusão em cascata).

### Funcionalidades extras

- **Verificação de e-mail e CPF no cadastro** — índices únicos no banco + checagem no
  `ContaController` + validação remota via AJAX (`[Remote]`) enquanto o usuário digita.
- **Recuperação de senha** — em `/Conta/EsqueciSenha` o usuário informa **e-mail + CPF**;
  se os dois baterem com um cadastro, o sistema gera um **token de uso único** (válido por
  30 min) e leva para a tela de nova senha. Se não baterem, mostra
  *"Os dados informados estão incorretos ou você não tem cadastro conosco."*
- **Notificações internas** — `LembreteConsultaService` roda em segundo plano e, para toda
  consulta que acontece nas **próximas 24 h**, cria uma notificação ("um dia antes").
  O sino no menu mostra o número de não lidas; a central fica em `/Notificacoes`.

### Validação (Data Annotations)

`[Required]`, `[EmailAddress]`, `[StringLength]`, `[Compare]`, `[DataType]`, `[Remote]`
e o atributo customizado `[Cpf]` (confere os dígitos verificadores) — validação executada
no servidor (`ModelState`) e no cliente.

### Segurança

- Senhas nunca são armazenadas em texto puro (`IPasswordHasher<Usuario>`).
- `ConsultasController` e `NotificacoesController` são decorados com `[Authorize]`.
- Cada usuário só acessa/edita/exclui **os próprios** registros (filtro por `UsuarioId`).
- `[ValidateAntiForgeryToken]` em todos os POST.
- Token de redefinição de senha aleatório (`RandomNumberGenerator`), de uso único e com expiração.
- No `Program.cs`, `app.UseAuthentication()` é chamado **antes** de `app.UseAuthorization()`.

---

## Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server — qualquer uma das opções:
  - **SQL Server Express** (instância `.\SQLEXPRESS`) — usado por padrão
  - SQL Server LocalDB / Developer
- `dotnet-ef` — **já vem no projeto** como ferramenta local (`.config/dotnet-tools.json`,
  versão 8.0.8). Basta restaurar:
  ```bash
  dotnet tool restore
  ```

## Configuração do Banco de Dados

### 1. Ajustar a Connection String

Arquivo [`ConsultasUVV/appsettings.json`](ConsultasUVV/appsettings.json):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ConsultasUVV;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Se usar LocalDB, troque por:
`Server=(localdb)\\MSSQLLocalDB;Database=ConsultasUVV;Trusted_Connection=True;TrustServerCertificate=True`

### 2. Criar o banco (Migrations)

O repositório **já inclui** a migration `InitialCreate`. Basta aplicá-la:

**CLI (.NET):**
```bash
cd ConsultasUVV
dotnet tool restore
dotnet ef database update
```

**Package Manager Console (Visual Studio):**
```powershell
Update-Database
```

O repositório traz duas migrations (`InitialCreate` e `AddCpfResetSenhaNotificacoes`);
`Update-Database` aplica as duas.

> Para testar os lembretes rapidamente: cadastre uma consulta para **daqui a poucas horas**
> (dentro de 24 h). Em até 1 minuto o serviço em segundo plano cria a notificação e o
> sino do menu passa a mostrar o contador.

## Executando a aplicação

**CLI:**
```bash
cd ConsultasUVV
dotnet run
```

**Visual Studio:** abra `ConsultasUVV.sln` e pressione F5.

Acesse `https://localhost:7080` (ou a porta indicada no console).

## Fluxo de uso

1. **Cadastrar** — `/Conta/Registrar`: nome, e-mail, **CPF**, senha (POST, senha com hash).
2. **Login** — `/Conta/Login`: autentica e gera o cookie de sessão.
3. **Esqueci minha senha** — `/Conta/EsqueciSenha`: e-mail + CPF → nova senha (com token).
4. **Consultas** — `/Consultas` (protegida por `[Authorize]`):
   - Criar (`GET/POST /Consultas/Create`)
   - Listar (`GET /Consultas`)
   - Detalhar (`GET /Consultas/Details/{id}`)
   - Editar (`GET/POST /Consultas/Edit/{id}`)
   - Excluir (`GET/POST /Consultas/Delete/{id}`)
5. **Notificações** — `/Notificacoes`: lembretes de consultas próximas (sino no menu).
6. **Sair** — botão "Sair" (POST `/Conta/Logout`).

## Mapa de verbos HTTP

| Ação | Verbo | Rota |
|------|-------|------|
| Tela de cadastro | GET | `/Conta/Registrar` |
| Efetivar cadastro | POST | `/Conta/Registrar` |
| Checar e-mail/CPF (AJAX) | GET | `/Conta/EmailDisponivel`, `/Conta/CpfDisponivel` |
| Tela de login | GET | `/Conta/Login` |
| Autenticar | POST | `/Conta/Login` |
| Logout | POST | `/Conta/Logout` |
| Esqueci a senha (form / envio) | GET / POST | `/Conta/EsqueciSenha` |
| Redefinir senha (form / envio) | GET / POST | `/Conta/RedefinirSenha` |
| Listar notificações | GET | `/Notificacoes` |
| Marcar todas como lidas | POST | `/Notificacoes/MarcarTodasComoLidas` |
| Remover notificação | POST | `/Notificacoes/Excluir/{id}` |
| Listar consultas | GET | `/Consultas` |
| Nova consulta (form) | GET | `/Consultas/Create` |
| Criar consulta | POST | `/Consultas/Create` |
| Editar (form) | GET | `/Consultas/Edit/{id}` |
| Atualizar consulta | POST | `/Consultas/Edit/{id}` |
| Excluir (confirmação) | GET | `/Consultas/Delete/{id}` |
| Excluir consulta | POST | `/Consultas/Delete/{id}` |
