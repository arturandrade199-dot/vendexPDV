using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class ContaPagarService : IContaPagarService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContaPagarService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<ContaPagar>> ListarAsync() => _unitOfWork.ContasPagar.ObterTodosAsync();

    public async Task<ResumoContasPagar> ObterResumoAsync()
    {
        var todas = await ListarAsync();
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);
        var noPeriodo = todas.Where(c => c.DataVencimento.Date >= inicioMes && c.DataVencimento.Date <= fimMes).ToList();

        var vencidos = todas.Where(c => c.Status != StatusContaFinanceira.Pago && c.DataVencimento.Date < hoje).Sum(c => c.ValorTotal);
        var vencemHoje = todas.Where(c => c.Status != StatusContaFinanceira.Pago && c.DataVencimento.Date == hoje).Sum(c => c.ValorTotal);
        var aVencer = todas.Where(c => c.Status != StatusContaFinanceira.Pago && c.DataVencimento.Date > hoje).Sum(c => c.ValorTotal);
        var pagos = noPeriodo.Where(c => c.Status == StatusContaFinanceira.Pago).Sum(c => c.ValorTotal);
        var totalPeriodo = noPeriodo.Sum(c => c.ValorTotal);

        return new ResumoContasPagar(vencidos, vencemHoje, aVencer, pagos, totalPeriodo);
    }

    public async Task<ContaPagar> AdicionarAsync(string descricao, string categoria, decimal valorTotal, DateTime dataVencimento)
    {
        var conta = new ContaPagar
        {
            Descricao = descricao,
            Categoria = categoria,
            ValorTotal = valorTotal,
            DataLancamento = DateTime.Now,
            DataVencimento = dataVencimento,
            Status = StatusContaFinanceira.Aberto
        };

        await _unitOfWork.ContasPagar.AdicionarAsync(conta);
        await _unitOfWork.SalvarAlteracoesAsync();
        return conta;
    }

    public async Task MarcarComoPagoAsync(int contaPagarId)
    {
        var conta = await _unitOfWork.ContasPagar.ObterPorIdAsync(contaPagarId);
        if (conta is null || conta.Status == StatusContaFinanceira.Pago)
            return;

        var pagamento = new ContaPagarPagamento
        {
            ContaPagarId = contaPagarId,
            ValorPago = conta.ValorTotal,
            DataPagamento = DateTime.Now,
            FormaPagamento = FormaPagamento.Dinheiro
        };

        await _unitOfWork.ContasPagarPagamentos.AdicionarAsync(pagamento);

        conta.Status = StatusContaFinanceira.Pago;
        _unitOfWork.ContasPagar.Atualizar(conta);

        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
