using Vendex.Domain.Enums;

namespace Vendex.Application.Services;

public record VarianteInput(int? Id, string Nome, string? CodigoBarras, decimal EstoqueAtual);

public record ProdutoInput(
    string Nome,
    string? CodigoBarras,
    decimal PrecoCusto,
    decimal PrecoVenda,
    decimal EstoqueAtual,
    UnidadeMedida UnidadeMedida,
    bool TemGrade,
    IReadOnlyList<VarianteInput> Variantes);
