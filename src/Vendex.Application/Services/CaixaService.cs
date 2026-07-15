using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class CaixaService : ICaixaService
{
    private readonly IUnitOfWork _unitOfWork;

    public CaixaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<Caixa?> ObterCaixaAbertoAsync() => _unitOfWork.Caixas.ObterCaixaAbertoAsync();

    public async Task<decimal> ObterValorEsperadoEmCaixaAsync()
    {
        var caixa = await _unitOfWork.Caixas.ObterCaixaAbertoAsync();
        if (caixa is null)
            throw new InvalidOperationException("Não há caixa aberto.");

        return await CalcularEsperadoEmCaixaAsync(caixa, DateTime.Now);
    }

    private async Task<decimal> CalcularEsperadoEmCaixaAsync(Caixa caixa, DateTime dataReferencia)
    {
        var vendas = await _unitOfWork.Vendas.ObterPorPeriodoAsync(caixa.DataAbertura, dataReferencia);
        var vendasDinheiro = vendas.Sum(v => v.Pagamentos.Where(p => p.FormaPagamento == FormaPagamento.Dinheiro).Sum(p => p.Valor));
        var reforcos = caixa.Movimentacoes.Where(m => m.Tipo == TipoMovimentacaoCaixa.Reforco).Sum(m => m.Valor);
        var sangrias = caixa.Movimentacoes.Where(m => m.Tipo == TipoMovimentacaoCaixa.Sangria).Sum(m => m.Valor);
        return caixa.ValorAberturaTotal + vendasDinheiro + reforcos - sangrias;
    }

    public async Task RegistrarMovimentacaoAsync(int usuarioId, TipoMovimentacaoCaixa tipo, decimal valor, string motivo)
    {
        if (valor <= 0)
            throw new InvalidOperationException("Informe um valor maior que zero.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new InvalidOperationException("Informe o motivo.");

        var caixa = await _unitOfWork.Caixas.ObterCaixaAbertoAsync();
        if (caixa is null)
            throw new InvalidOperationException("Não há caixa aberto.");

        caixa.Movimentacoes.Add(new CaixaMovimentacao
        {
            CaixaId = caixa.Id,
            Tipo = tipo,
            Valor = valor,
            Motivo = motivo.Trim(),
            UsuarioId = usuarioId,
            DataHora = DateTime.Now
        });

        _unitOfWork.Caixas.Atualizar(caixa);
        await _unitOfWork.SalvarAlteracoesAsync();
    }

    public async Task<Caixa> AbrirCaixaAsync(int usuarioId, IReadOnlyList<ContagemCedula> contagem)
    {
        var caixaAberto = await _unitOfWork.Caixas.ObterCaixaAbertoAsync();
        if (caixaAberto is not null)
            throw new InvalidOperationException("Já existe um caixa aberto.");

        var caixa = new Caixa
        {
            DataAbertura = DateTime.Now,
            UsuarioAberturaId = usuarioId
        };

        foreach (var item in contagem.Where(c => c.Quantidade > 0))
        {
            caixa.AberturaDetalhes.Add(new CaixaAberturaDetalhe
            {
                TipoCedula = item.TipoCedula,
                Quantidade = item.Quantidade,
                Subtotal = item.TipoCedula * item.Quantidade
            });
        }

        caixa.ValorAberturaTotal = caixa.AberturaDetalhes.Sum(d => d.Subtotal);

        await _unitOfWork.Caixas.AdicionarAsync(caixa);
        await _unitOfWork.SalvarAlteracoesAsync();
        return caixa;
    }

    public async Task<ResumoFechamentoCaixa> FecharCaixaAsync(int usuarioId, IReadOnlyList<ContagemCedula> contagem)
    {
        var caixa = await _unitOfWork.Caixas.ObterCaixaAbertoAsync();
        if (caixa is null)
            throw new InvalidOperationException("Não há caixa aberto para fechar.");

        var dataFechamento = DateTime.Now;
        var vendas = await _unitOfWork.Vendas.ObterPorPeriodoAsync(caixa.DataAbertura, dataFechamento);

        var faturamentoTotal = vendas.Sum(v => v.ValorTotal);
        var custoTotal = vendas.Sum(v => v.Itens.Sum(i => i.PrecoCustoUnitario * i.Quantidade));
        var lucroTotal = faturamentoTotal - custoTotal;

        var esperadoEmCaixa = await CalcularEsperadoEmCaixaAsync(caixa, dataFechamento);

        foreach (var item in contagem.Where(c => c.Quantidade > 0))
        {
            caixa.FechamentoDetalhes.Add(new CaixaFechamentoDetalhe
            {
                TipoCedula = item.TipoCedula,
                Quantidade = item.Quantidade,
                Subtotal = item.TipoCedula * item.Quantidade
            });
        }

        var valorFechamentoTotal = caixa.FechamentoDetalhes.Sum(d => d.Subtotal);
        var divergencia = valorFechamentoTotal - esperadoEmCaixa;

        caixa.DataFechamento = dataFechamento;
        caixa.UsuarioFechamentoId = usuarioId;
        caixa.ValorFechamentoTotal = valorFechamentoTotal;
        caixa.FaturamentoTotal = faturamentoTotal;
        caixa.CustoTotal = custoTotal;
        caixa.LucroTotal = lucroTotal;
        caixa.Status = StatusCaixa.Fechado;

        _unitOfWork.Caixas.Atualizar(caixa);
        await _unitOfWork.SalvarAlteracoesAsync();

        return new ResumoFechamentoCaixa(
            caixa.DataAbertura,
            dataFechamento,
            caixa.ValorAberturaTotal,
            valorFechamentoTotal,
            esperadoEmCaixa,
            divergencia,
            faturamentoTotal,
            custoTotal,
            lucroTotal);
    }
}
