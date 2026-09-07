using System.Security.Claims;
using System.Security.Cryptography;
using ConsultasUVV.Data;
using ConsultasUVV.Models;
using ConsultasUVV.Models.ViewModels;
using ConsultasUVV.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultasUVV.Controllers;

public class ContaController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public ContaController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public IActionResult Registrar() => View(new RegistroViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistroViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        var cpf = CpfAttribute.Normalizar(model.Cpf);

        if (await _context.Usuarios.AnyAsync(u => u.Email == email))
            ModelState.AddModelError(nameof(model.Email), "Este e-mail já está cadastrado.");

        if (await _context.Usuarios.AnyAsync(u => u.Cpf == cpf))
            ModelState.AddModelError(nameof(model.Cpf), "Este CPF já está cadastrado.");

        if (!ModelState.IsValid)
            return View(model);

        var usuario = new Usuario
        {
            Nome = model.Nome.Trim(),
            Email = email,
            Cpf = cpf,
            DataCadastro = DateTime.Now
        };
        usuario.Senha = _passwordHasher.HashPassword(usuario, model.Senha);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Cadastro realizado com sucesso. Faça login para continuar.";
        return RedirectToAction(nameof(Login));
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> EmailDisponivel(string email)
    {
        var normalizado = (email ?? string.Empty).Trim().ToLowerInvariant();
        var existe = await _context.Usuarios.AnyAsync(u => u.Email == normalizado);
        return Json(existe ? "Este e-mail já está cadastrado." : "true");
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> CpfDisponivel(string cpf)
    {
        var digitos = CpfAttribute.Normalizar(cpf);
        var existe = await _context.Usuarios.AnyAsync(u => u.Cpf == digitos);
        return Json(existe ? "Este CPF já está cadastrado." : "true");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario is null ||
            _passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, model.Senha)
                == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            return View(model);
        }

        await SignInAsync(usuario, model.LembrarDeMim);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Consultas");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult EsqueciSenha() => View(new EsqueciSenhaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EsqueciSenha(EsqueciSenhaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim().ToLowerInvariant();
        var cpf = CpfAttribute.Normalizar(model.Cpf);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email && u.Cpf == cpf);

        if (usuario is null)
        {
            ModelState.AddModelError(string.Empty,
                "Os dados informados estão incorretos ou você não tem cadastro conosco.");
            return View(model);
        }

        usuario.TokenResetSenha = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        usuario.TokenResetExpiraEm = DateTime.Now.AddMinutes(30);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(RedefinirSenha), new
        {
            token = usuario.TokenResetSenha,
            email = usuario.Email
        });
    }

    [HttpGet]
    public async Task<IActionResult> RedefinirSenha(string? token, string? email)
    {
        if (await BuscarPorToken(token, email) is null)
            return View("LinkInvalido");

        return View(new RedefinirSenhaViewModel
        {
            Token = token!,
            Email = email!
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = await BuscarPorToken(model.Token, model.Email);
        if (usuario is null)
            return View("LinkInvalido");

        usuario.Senha = _passwordHasher.HashPassword(usuario, model.NovaSenha);
        usuario.TokenResetSenha = null;
        usuario.TokenResetExpiraEm = null;
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Senha redefinida com sucesso. Faça login com a nova senha.";
        return RedirectToAction(nameof(Login));
    }

    private async Task<Usuario?> BuscarPorToken(string? token, string? email)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            return null;

        var emailNormalizado = email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == emailNormalizado && u.TokenResetSenha == token);

        if (usuario?.TokenResetExpiraEm is null || usuario.TokenResetExpiraEm < DateTime.Now)
            return null;

        return usuario;
    }

    private Task SignInAsync(Usuario usuario, bool persistente)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var props = new AuthenticationProperties { IsPersistent = persistente };

        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            props);
    }
}
