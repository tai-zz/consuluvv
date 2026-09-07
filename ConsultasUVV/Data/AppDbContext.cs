using ConsultasUVV.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsultasUVV.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Nome).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(150);
            e.Property(u => u.Cpf).HasMaxLength(11).IsFixedLength();
            e.Property(u => u.TokenResetSenha).HasMaxLength(64);

            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Cpf).IsUnique();
        });

        modelBuilder.Entity<Consulta>(e =>
        {
            e.Property(c => c.Especialidade).HasMaxLength(100);
            e.Property(c => c.Descricao).HasMaxLength(500);

            e.HasOne(c => c.Usuario)
                .WithMany(u => u.Consultas)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notificacao>(e =>
        {
            e.Property(n => n.Mensagem).HasMaxLength(300);

            e.HasOne(n => n.Usuario)
                .WithMany(u => u.Notificacoes)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.Consulta)
                .WithMany()
                .HasForeignKey(n => n.ConsultaId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasIndex(n => n.ConsultaId).IsUnique();
        });
    }
}
