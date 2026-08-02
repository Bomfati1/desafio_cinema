namespace Cinema.Api.Models;

/// <summary>
/// Resultado paginado genérico para endpoints de listagem.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>
/// Constantes compartilhadas para endpoints paginados.
/// </summary>
public static class PaginationDefaults
{
    public const int MaxPageSize = 100;
}
