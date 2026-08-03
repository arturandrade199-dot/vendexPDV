using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

/// <summary>Controle de validade — deliberadamente independente do fluxo de venda: cadastrar
/// um lote ou registrar uma perda não mexem em Produto.EstoqueAtual. É uma camada paralela de
/// acompanhamento de validade/perda, não uma fonte de verdade de estoque (evita duplicar
/// contagem, já que hoje só venda/devolução/edição manual mudam o estoque).</summary>
public class LoteService : ILoteService
{
    private readonly IUnitOfWork _unitOfWork;

    public LoteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<Lote>> ListarAsync() => _unitOfWork.Lotes.ObterTodosComProdutoAsync();

    public async Task<ResumoLotes> ObterResumoAsync()
    {
        var todos = await ListarAsync();
        var vencidos = todos.Count(l => l.SituacaoEfetiva() == SituacaoLote.Vencido);
        var vencemEm3Dias = todos.Count(l => l.SituacaoEfetiva(3) == SituacaoLote.Vencendo);
        var vencemEm7Dias = todos.Count(l => l.SituacaoEfetiva(7) == SituacaoLote.Vencendo);

        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);
        var perdasNoMes = await _unitOfWork.Lotes.ObterPerdasPorPeriodoAsync(inicioMes, fimMes);
        var perdidoNoPeriodo = perdasNoMes.Sum(p => p.ValorPerdido);

        return new ResumoLotes(vencidos, vencemEm3Dias, vencemEm7Dias, perdidoNoPeriodo);
    }

    public async Task<Lote> RegistrarAsync(
        int produtoId, int? produtoVarianteId, DateTime dataFabricacao, DateTime dataValidade,
        decimal quantidade, string? observacoes)
    {
        if (quantidade <= 0)
            throw new InvalidOperationException("Informe uma quantidade maior que zero.");

        if (dataValidade.Date < dataFabricacao.Date)
            throw new InvalidOperationException("A validade não pode ser anterior à data de fabricação/entrada.");

        var lote = new Lote
        {
            ProdutoId = produtoId,
            ProdutoVarianteId = produtoVarianteId,
            DataFabricacao = dataFabricacao,
            DataValidade = dataValidade,
            QuantidadeInicial = quantidade,
            QuantidadeAtual = quantidade,
            Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim()
        };

        await _unitOfWork.Lotes.AdicionarAsync(lote);
        await _unitOfWork.SalvarAlteracoesAsync();
        return lote;
    }

    public async Task<LotePerda> RegistrarPerdaAsync(int loteId, decimal quantidade, string? motivo)
    {
        var lote = await _unitOfWork.Lotes.ObterComProdutoAsync(loteId)
            ?? throw new InvalidOperationException("Lote não encontrado.");

        if (quantidade <= 0)
            throw new InvalidOperationException("Informe uma quantidade maior que zero.");

        if (quantidade > lote.QuantidadeAtual)
            throw new InvalidOperationException("Não é possível perder mais do que a quantidade restante no lote.");

        var perda = new LotePerda
        {
            LoteId = lote.Id,
            Quantidade = quantidade,
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim(),
            PrecoCustoUnitarioNaData = lote.Produto.PrecoCusto,
            ValorPerdido = quantidade * lote.Produto.PrecoCusto
        };

        lote.QuantidadeAtual -= quantidade;
        lote.Perdas.Add(perda);
        _unitOfWork.Lotes.Atualizar(lote);

        await _unitOfWork.SalvarAlteracoesAsync();
        return perda;
    }

    public async Task RemoverAsync(int loteId)
    {
        var lote = await _unitOfWork.Lotes.ObterComProdutoAsync(loteId)
            ?? throw new InvalidOperationException("Lote não encontrado.");

        // Preserva o histórico que já pode ter aparecido num relatório de perdas — remover o
        // lote apagaria o contexto (produto/validade) por trás de cada LotePerda registrada.
        if (lote.Perdas.Count > 0)
            throw new InvalidOperationException("Este lote já tem perdas registradas e não pode ser removido.");

        _unitOfWork.Lotes.Remover(lote);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
