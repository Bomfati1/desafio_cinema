using Microsoft.EntityFrameworkCore.Storage;

namespace Cinema.Api.Data;

/// <summary>
/// Abstração sobre transações e SaveChanges do EF Core.
/// Permite que serviços coordenem operações atômicas sem depender
/// diretamente do AppDbContext (DIP — Dependency Inversion Principle).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
