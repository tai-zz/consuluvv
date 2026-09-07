using System.Security.Claims;
using ConsultasUVV.Data;
using ConsultasUVV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsultasUVV.Controllers;

[Authorize]
public class ConsultasController : Controller
{
    private readonly AppDbContext _context;

    public ConsultasController(AppDbContext context)
    {
        _context = context;
    }

    private int UsuarioIdAtual =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var consultas = await _context.Consultas
            .Where(c => c.UsuarioId == UsuarioIdAtual)
            .OrderBy(c => c.DataHora)
            .AsNoTracking()
            .ToListAsync();

        return View(consultas);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var consulta = await _context.Consultas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioIdAtual);

        if (consulta is null) return NotFound();

        return View(consulta);
    }

    public IActionResult Create() => View(new Consulta { DataHora = DateTime.Now });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Especialidade,DataHora,Descricao")] Consulta consulta)
    {
        if (!ModelState.IsValid)
            return View(consulta);

        consulta.UsuarioId = UsuarioIdAtual;
        _context.Add(consulta);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Consulta registrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var consulta = await _context.Consultas
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioIdAtual);

        if (consulta is null) return NotFound();

        return View(consulta);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Especialidade,DataHora,Descricao")] Consulta consulta)
    {
        if (id != consulta.Id) return NotFound();

        var existente = await _context.Consultas
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioIdAtual);

        if (existente is null) return NotFound();

        if (!ModelState.IsValid)
            return View(consulta);

        existente.Especialidade = consulta.Especialidade;
        existente.DataHora = consulta.DataHora;
        existente.Descricao = consulta.Descricao;

        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Consulta atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var consulta = await _context.Consultas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioIdAtual);

        if (consulta is null) return NotFound();

        return View(consulta);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var consulta = await _context.Consultas
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == UsuarioIdAtual);

        if (consulta is not null)
        {
            var notificacoes = await _context.Notificacoes
                .Where(n => n.ConsultaId == consulta.Id)
                .ToListAsync();
            _context.Notificacoes.RemoveRange(notificacoes);

            _context.Consultas.Remove(consulta);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Consulta excluída com sucesso.";
        }

        return RedirectToAction(nameof(Index));
    }
}
