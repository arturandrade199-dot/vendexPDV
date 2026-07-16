using System.Globalization;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class RelatorioService : IRelatorioService
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IUnitOfWork _unitOfWork;

    public RelatorioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<RelatorioResultado> GerarAsync(TipoRelatorio tipo, DateTime? inicio, DateTime? fim) => tipo switch
    {
        TipoRelatorio.EstoqueProdutos => GerarEstoqueProdutosAsync(),
        TipoRelatorio.ProdutosMaisVendidos => GerarProdutosVendidosAsync(ExigirPeriodo(inicio, fim), maisVendidos: true),
        TipoRelatorio.ProdutosMenosVendidos => GerarProdutosVendidosAsync(ExigirPeriodo(inicio, fim), maisVendidos: false),
        TipoRelatorio.ContasPagas => GerarContasPagasAsync(ExigirPeriodo(inicio, fim)),
        TipoRelatorio.ContasAPagar => GerarContasAPagarAsync(ExigirPeriodo(inicio, fim)),
        TipoRelatorio.ContasRecebidas => GerarContasRecebidasAsync(ExigirPeriodo(inicio, fim)),
        TipoRelatorio.ContasAReceber => GerarContasAReceberAsync(ExigirPeriodo(inicio, fim)),
        TipoRelatorio.AberturasCaixa => GerarCaixasAsync(ExigirPeriodo(inicio, fim), aberturas: true),
        TipoRelatorio.FechamentosCaixa => GerarCaixasAsync(ExigirPeriodo(inicio, fim), aberturas: false),
        TipoRelatorio.VendasPorFormaPagamento => GerarVendasPorFormaPagamentoAsync(ExigirPeriodo(inicio, fim)),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private static (DateTime Inicio, DateTime Fim) ExigirPeriodo(DateTime? inicio, DateTime? fim)
    {
        if (inicio is null || fim is null)
            throw new InvalidOperationException("Informe o período (data inicial e final).");

        // Inclui o dia inteiro do "fim" — sem isso, um DatePicker com hora 00:00 excluiria
        // qualquer coisa lançada depois da meia-noite do próprio dia final.
        return (inicio.Value.Date, fim.Value.Date.AddDays(1).AddTicks(-1));
    }

    private async Task<RelatorioResultado> GerarEstoqueProdutosAsync()
    {
        var produtos = await _unitOfWork.Produtos.ObterTodosAsync();

        var linhas = produtos
            .OrderBy(p => p.Nome)
            .Select(p => (IReadOnlyList<string>)new[]
            {
                p.Nome,
                p.EstoqueAtual.ToString(CulturaBr),
                p.PrecoCusto.ToString("C2", CulturaBr),
                p.PrecoVenda.ToString("C2", CulturaBr),
                (p.EstoqueAtual * p.PrecoCusto).ToString("C2", CulturaBr)
            })
            .ToList();

        var totalEstoque = produtos.Sum(p => p.EstoqueAtual * p.PrecoCusto);

        return new RelatorioResultado(
            "Estoque de produtos",
            new[]
            {
                new ColunaRelatorio("Produto"),
                new ColunaRelatorio("Estoque", AlinhadaDireita: true),
                new ColunaRelatorio("Preço de custo", AlinhadaDireita: true),
                new ColunaRelatorio("Preço de venda", AlinhadaDireita: true),
                new ColunaRelatorio("Valor em estoque (custo)", AlinhadaDireita: true)
            },
            linhas,
            new[] { ("Valor total em estoque (custo)", totalEstoque.ToString("C2", CulturaBr)) });
    }

    private async Task<RelatorioResultado> GerarProdutosVendidosAsync((DateTime Inicio, DateTime Fim) periodo, bool maisVendidos)
    {
        var vendas = await _unitOfWork.Vendas.ObterPorPeriodoAsync(periodo.Inicio, periodo.Fim);
        var agregados = vendas
            .SelectMany(v => v.Itens)
            .GroupBy(i => i.ProdutoId)
            .ToDictionary(g => g.Key, g => (Quantidade: g.Sum(i => i.Quantidade), Total: g.Sum(i => i.Subtotal), Nome: g.First().Produto.Nome));

        List<(string Nome, int Quantidade, decimal Total)> linhasBase;

        if (maisVendidos)
        {
            linhasBase = agregados.Values
                .Select(a => (a.Nome, a.Quantidade, a.Total))
                .OrderByDescending(a => a.Quantidade)
                .ToList();
        }
        else
        {
            // Parte da lista completa de produtos (não só dos que venderam) — o dado mais
            // útil desse relatório é justamente o que NÃO saiu nenhuma vez no período.
            var produtos = await _unitOfWork.Produtos.ObterTodosAsync();
            linhasBase = produtos
                .Select(p => agregados.TryGetValue(p.Id, out var a)
                    ? (Nome: p.Nome, Quantidade: a.Quantidade, Total: a.Total)
                    : (Nome: p.Nome, Quantidade: 0, Total: 0m))
                .OrderBy(a => a.Quantidade)
                .ToList();
        }

        var linhas = linhasBase
            .Select(a => (IReadOnlyList<string>)new[] { a.Nome, a.Quantidade.ToString(CulturaBr), a.Total.ToString("C2", CulturaBr) })
            .ToList();

        return new RelatorioResultado(
            maisVendidos ? "Produtos que mais venderam" : "Produtos com menos saída",
            new[]
            {
                new ColunaRelatorio("Produto"),
                new ColunaRelatorio("Quantidade vendida", AlinhadaDireita: true),
                new ColunaRelatorio("Total vendido", AlinhadaDireita: true)
            },
            linhas);
    }

    private async Task<RelatorioResultado> GerarContasPagasAsync((DateTime Inicio, DateTime Fim) periodo)
    {
        var pagamentos = await _unitOfWork.ContasPagar.ObterPagamentosPorPeriodoAsync(periodo.Inicio, periodo.Fim);

        var linhas = pagamentos
            .OrderBy(p => p.DataPagamento)
            .Select(p => (IReadOnlyList<string>)new[]
            {
                p.DataPagamento.ToString("dd/MM/yyyy", CulturaBr),
                p.ContaPagar.Descricao,
                p.ContaPagar.Fornecedor?.Nome ?? "—",
                p.FormaPagamento.ParaTexto(),
                p.ValorPago.ToString("C2", CulturaBr)
            })
            .ToList();

        var total = pagamentos.Sum(p => p.ValorPago);

        return new RelatorioResultado(
            "Contas pagas no período",
            new[]
            {
                new ColunaRelatorio("Data"), new ColunaRelatorio("Descrição"), new ColunaRelatorio("Fornecedor"),
                new ColunaRelatorio("Forma de pagamento"), new ColunaRelatorio("Valor pago", AlinhadaDireita: true)
            },
            linhas,
            new[] { ("Total pago", total.ToString("C2", CulturaBr)) });
    }

    private async Task<RelatorioResultado> GerarContasAPagarAsync((DateTime Inicio, DateTime Fim) periodo)
    {
        var contas = await _unitOfWork.ContasPagar.ObterPorPeriodoAsync(periodo.Inicio, periodo.Fim);

        var linhas = contas
            .OrderBy(c => c.DataVencimento)
            .Select(c => (IReadOnlyList<string>)new[]
            {
                c.DataVencimento.ToString("dd/MM/yyyy", CulturaBr),
                c.Descricao,
                c.Fornecedor?.Nome ?? "—",
                FormatarStatus(c.Status),
                c.ValorTotal.ToString("C2", CulturaBr)
            })
            .ToList();

        var total = contas.Sum(c => c.ValorTotal);

        return new RelatorioResultado(
            "Contas a pagar no período",
            new[]
            {
                new ColunaRelatorio("Vencimento"), new ColunaRelatorio("Descrição"), new ColunaRelatorio("Fornecedor"),
                new ColunaRelatorio("Situação"), new ColunaRelatorio("Valor", AlinhadaDireita: true)
            },
            linhas,
            new[] { ("Total", total.ToString("C2", CulturaBr)) });
    }

    private async Task<RelatorioResultado> GerarContasRecebidasAsync((DateTime Inicio, DateTime Fim) periodo)
    {
        var pagamentos = await _unitOfWork.ContasReceber.ObterPagamentosPorPeriodoAsync(periodo.Inicio, periodo.Fim);

        var linhas = pagamentos
            .OrderBy(p => p.DataPagamento)
            .Select(p => (IReadOnlyList<string>)new[]
            {
                p.DataPagamento.ToString("dd/MM/yyyy", CulturaBr),
                p.ContaReceber.Descricao,
                p.ContaReceber.Cliente.Nome,
                p.FormaPagamento.ParaTexto(),
                p.ValorPago.ToString("C2", CulturaBr)
            })
            .ToList();

        var total = pagamentos.Sum(p => p.ValorPago);

        return new RelatorioResultado(
            "Contas recebidas no período",
            new[]
            {
                new ColunaRelatorio("Data"), new ColunaRelatorio("Descrição"), new ColunaRelatorio("Cliente"),
                new ColunaRelatorio("Forma de pagamento"), new ColunaRelatorio("Valor recebido", AlinhadaDireita: true)
            },
            linhas,
            new[] { ("Total recebido", total.ToString("C2", CulturaBr)) });
    }

    private async Task<RelatorioResultado> GerarContasAReceberAsync((DateTime Inicio, DateTime Fim) periodo)
    {
        var contas = await _unitOfWork.ContasReceber.ObterPorPeriodoAsync(periodo.Inicio, periodo.Fim);

        var linhas = contas
            .OrderBy(c => c.DataVencimento)
            .Select(c => (IReadOnlyList<string>)new[]
            {
                c.DataVencimento.ToString("dd/MM/yyyy", CulturaBr),
                c.Descricao,
                c.Cliente.Nome,
                FormatarStatus(c.Status),
                c.ValorTotal.ToString("C2", CulturaBr)
            })
            .ToList();

        var total = contas.Sum(c => c.ValorTotal);

        return new RelatorioResultado(
            "Contas a receber no período",
            new[]
            {
                new ColunaRelatorio("Vencimento"), new ColunaRelatorio("Descrição"), new ColunaRelatorio("Cliente"),
                new ColunaRelatorio("Situação"), new ColunaRelatorio("Valor", AlinhadaDireita: true)
            },
            linhas,
            new[] { ("Total", total.ToString("C2", CulturaBr)) });
    }

    private async Task<RelatorioResultado> GerarCaixasAsync((DateTime Inicio, DateTime Fim) periodo, bool aberturas)
    {
        var caixas = await _unitOfWork.Caixas.ObterPorPeriodoAsync(periodo.Inicio, periodo.Fim);
        if (!aberturas)
            caixas = caixas.Where(c => c.DataFechamento.HasValue).ToList();

        if (aberturas)
        {
            var linhasAbertura = caixas
                .OrderBy(c => c.DataAbertura)
                .Select(c => (IReadOnlyList<string>)new[]
                {
                    c.DataAbertura.ToString("dd/MM/yyyy HH:mm", CulturaBr),
                    c.UsuarioAbertura.Nome,
                    c.ValorAberturaTotal.ToString("C2", CulturaBr),
                    c.Status == StatusCaixa.Aberto ? "Aberto" : "Fechado"
                })
                .ToList();

            return new RelatorioResultado(
                "Abertura de caixas no período",
                new[]
                {
                    new ColunaRelatorio("Data/hora abertura"), new ColunaRelatorio("Usuário"),
                    new ColunaRelatorio("Valor de abertura", AlinhadaDireita: true), new ColunaRelatorio("Situação")
                },
                linhasAbertura);
        }

        var linhasFechamento = caixas
            .OrderBy(c => c.DataFechamento)
            .Select(c => (IReadOnlyList<string>)new[]
            {
                c.DataFechamento!.Value.ToString("dd/MM/yyyy HH:mm", CulturaBr),
                c.UsuarioFechamento?.Nome ?? "—",
                (c.ValorFechamentoTotal ?? 0).ToString("C2", CulturaBr),
                (c.LucroTotal ?? 0).ToString("C2", CulturaBr)
            })
            .ToList();

        var lucroTotal = caixas.Sum(c => c.LucroTotal ?? 0);

        return new RelatorioResultado(
            "Fechamento de caixa no período",
            new[]
            {
                new ColunaRelatorio("Data/hora fechamento"), new ColunaRelatorio("Usuário"),
                new ColunaRelatorio("Valor de fechamento", AlinhadaDireita: true), new ColunaRelatorio("Lucro do dia", AlinhadaDireita: true)
            },
            linhasFechamento,
            new[] { ("Lucro total do período", lucroTotal.ToString("C2", CulturaBr)) });
    }

    private async Task<RelatorioResultado> GerarVendasPorFormaPagamentoAsync((DateTime Inicio, DateTime Fim) periodo)
    {
        var vendas = await _unitOfWork.Vendas.ObterPorPeriodoAsync(periodo.Inicio, periodo.Fim);
        var agregados = vendas
            .SelectMany(v => v.Pagamentos)
            .GroupBy(p => p.FormaPagamento)
            .Select(g => (Forma: g.Key, Total: g.Sum(p => p.Valor)))
            .OrderByDescending(a => a.Total)
            .ToList();

        var linhas = agregados
            .Select(a => (IReadOnlyList<string>)new[] { a.Forma.ParaTexto(), a.Total.ToString("C2", CulturaBr) })
            .ToList();

        var total = agregados.Sum(a => a.Total);

        return new RelatorioResultado(
            "Total de vendas por forma de pagamento",
            new[] { new ColunaRelatorio("Forma de pagamento"), new ColunaRelatorio("Total", AlinhadaDireita: true) },
            linhas,
            new[] { ("Total geral", total.ToString("C2", CulturaBr)) });
    }

    private static string FormatarStatus(StatusContaFinanceira status) => status switch
    {
        StatusContaFinanceira.Pago => "Pago",
        StatusContaFinanceira.Atrasado => "Atrasado",
        StatusContaFinanceira.Parcial => "Parcial",
        _ => "Em aberto"
    };

    public byte[] ExportarExcel(RelatorioResultado resultado)
    {
        using var workbook = new XLWorkbook();
        var nomeAba = resultado.Titulo.Length > 31 ? resultado.Titulo[..31] : resultado.Titulo;
        var planilha = workbook.Worksheets.Add(nomeAba);

        planilha.Cell(1, 1).Value = resultado.Titulo;
        planilha.Cell(1, 1).Style.Font.Bold = true;
        planilha.Cell(1, 1).Style.Font.FontSize = 14;

        const int linhaCabecalho = 3;
        for (var c = 0; c < resultado.Colunas.Count; c++)
        {
            var celula = planilha.Cell(linhaCabecalho, c + 1);
            celula.Value = resultado.Colunas[c].Titulo;
            celula.Style.Font.Bold = true;
            celula.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDEBFF");
        }

        for (var l = 0; l < resultado.Linhas.Count; l++)
        {
            for (var c = 0; c < resultado.Linhas[l].Count; c++)
                planilha.Cell(linhaCabecalho + 1 + l, c + 1).Value = resultado.Linhas[l][c];
        }

        if (resultado.Totais is { Count: > 0 })
        {
            var linhaTotal = linhaCabecalho + 1 + resultado.Linhas.Count + 1;
            var coluna = 1;
            foreach (var (rotulo, valor) in resultado.Totais)
            {
                planilha.Cell(linhaTotal, coluna).Value = rotulo;
                planilha.Cell(linhaTotal, coluna).Style.Font.Bold = true;
                planilha.Cell(linhaTotal, coluna + 1).Value = valor;
                planilha.Cell(linhaTotal, coluna + 1).Style.Font.Bold = true;
                coluna += 3;
            }
        }

        planilha.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportarPdf(RelatorioResultado resultado)
    {
        var documento = Document.Create(container =>
        {
            container.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(30);
                pagina.DefaultTextStyle(x => x.FontSize(9));

                pagina.Header().PaddingBottom(10).Text(resultado.Titulo).FontSize(16).Bold();

                pagina.Content().Column(coluna =>
                {
                    coluna.Item().Table(tabela =>
                    {
                        tabela.ColumnsDefinition(definicao =>
                        {
                            foreach (var _ in resultado.Colunas)
                                definicao.RelativeColumn();
                        });

                        tabela.Header(cabecalho =>
                        {
                            foreach (var col in resultado.Colunas)
                                cabecalho.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(col.Titulo).Bold();
                        });

                        foreach (var linha in resultado.Linhas)
                        {
                            for (var i = 0; i < linha.Count; i++)
                            {
                                var celula = tabela.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
                                if (resultado.Colunas[i].AlinhadaDireita)
                                    celula.AlignRight().Text(linha[i]);
                                else
                                    celula.Text(linha[i]);
                            }
                        }
                    });

                    if (resultado.Totais is { Count: > 0 })
                    {
                        coluna.Item().PaddingTop(12).Column(totaisColuna =>
                        {
                            foreach (var (rotulo, valor) in resultado.Totais)
                                totaisColuna.Item().Text($"{rotulo}: {valor}").Bold().FontSize(11);
                        });
                    }
                });

                pagina.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return documento.GeneratePdf();
    }
}
