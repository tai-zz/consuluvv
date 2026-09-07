using System.Security.Claims;
using ConsultasUVV.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultasUVV.Controllers;

[Authorize]
public class NotificacoesController : Controller
{
    private readonly AppDbContext _context;

    public NotificacoesController(AppDbContext context)
    {
        _context = context;
    }

    private int UsuarioIdAtual =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var notificacoes = await _context.Notificacoes
            .Where(n => n.UsuarioId == UsuarioIdAtual)
            .OrderByDescending(n => n.CriadaEm)
            .AsNoTracking()
            .ToListAsync();

        return View(notificacoes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarTodasComoLidas()
    {
        var naoLidas = await _context.Notificacoes
            .Where(n => n.UsuarioId == UsuarioIdAtual && !n.Lida)
            .ToListAsync();

        foreach (var n in naoLidas)
            n.Lida = true;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var notificacao = await _context.Notificacoes
            .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == UsuarioIdAtual);

        if (notificacao is not null)
        {
            _context.Notificacoes.Remove(notificacao);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
