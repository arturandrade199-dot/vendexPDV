namespace Vendex.App.ViewModels;

public record ReciboVenda(
    int NumeroVenda,
    DateTime DataHora,
    IReadOnlyList<ItemCarrinhoViewModel> Itens,
    string TotalFormatado,
    IReadOnlyList<string> PagamentosTexto,
    string? TrocoTotalFormatado,
    string? ClienteTexto);
