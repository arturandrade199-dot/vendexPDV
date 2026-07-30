using Vendex.App.ViewModels;

namespace Vendex.App.Impressao;

/// <summary>DataContext de ReciboTermicoView — junta a venda com o cabeçalho de loja
/// (duas fontes diferentes) num único objeto bindável.</summary>
public record ReciboTermicoDados(ReciboVenda Recibo, DadosLojaImpressao? DadosLoja);
