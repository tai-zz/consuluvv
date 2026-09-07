using ConsultasUVV.Data;
using ConsultasUVV.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsultasUVV.Services;

public class LembreteConsultaService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan AntecedenciaLembrete = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LembreteConsultaService> _logger;

    public LembreteConsultaService(
        IServiceScopeFactory scopeFactory,
        ILogger<LembreteConsultaService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);

        do
        {
            try
            {
                await GerarLembretesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar lembretes de consulta.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task GerarLembretesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var agora = DateTime.Now;
        var limite = agora.Add(AntecedenciaLembrete);

        var consultas = await db.Consultas
            .Where(c => c.DataHora > agora
                        && c.DataHora <= limite
                        && !db.Notificacoes.Any(n => n.ConsultaId == c.Id))
            .ToListAsync(ct);

        if (consultas.Count == 0)
            return;

        foreach (var consulta in consultas)
        {
            db.Notificacoes.Add(new Notificacao
            {
                UsuarioId = consulta.UsuarioId,
                ConsultaId = consulta.Id,
                Mensagem = $"Lembrete: sua consulta de {consulta.Especialidade} é em " +
                           $"{consulta.DataHora:dd/MM/yyyy} às {consulta.DataHora:HH:mm}.",
                CriadaEm = agora,
                Lida = false
            });
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("{Qtd} lembrete(s) de consulta gerado(s).", consultas.Count);
    }
}
