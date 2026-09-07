using System.Security.Claims;
using ConsultasUVV.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultasUVV.ViewComponents;

public class NotificacoesBadgeViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public NotificacoesBadgeViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var idClaim = (User as ClaimsPrincipal)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(idClaim, out var usuarioId))
            return View(0);

        var naoLidas = await _context.Notificacoes
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Lida);

        return View(naoLidas);
    }
}
