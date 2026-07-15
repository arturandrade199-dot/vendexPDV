namespace Vendex.Application.Services;

public record ColunaRelatorio(string Titulo, bool AlinhadaDireita = false);

public record RelatorioResultado(
    string Titulo,
    IReadOnlyList<ColunaRelatorio> Colunas,
    IReadOnlyList<IReadOnlyList<string>> Linhas,
    IReadOnlyList<(string Rotulo, string Valor)>? Totais = null);
