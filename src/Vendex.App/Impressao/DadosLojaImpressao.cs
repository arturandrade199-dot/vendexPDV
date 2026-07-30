namespace Vendex.App.Impressao;

/// <summary>Cabeçalho de loja usado só no layout térmico do cupom — o layout moderno
/// (tela cheia) não tem esse cabeçalho, então essa informação não faz parte de
/// ReciboVenda, e sim é passada à parte na hora de montar o visual pra imprimir.</summary>
public record DadosLojaImpressao(string? Nome, string? Endereco, string? Cnpj);
