using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class ContaReceberService : IContaReceberService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContaReceberService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<ContaReceber>> ListarAsync() => _unitOfWork.ContasReceber.ObterTodosComClienteAsync();

    public async Task<ResumoContasReceber> ObterResumoAsync()
    {
        var todas = await ListarAsync();
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);
        var noPeriodo = todas.Where(c => c.DataVencimento.Date >= inicioMes && c.DataVencimento.Date <= fimMes).ToList();

        var vencidos = todas.Where(c => c.Status != StatusContaFinanceira.Pago && c.DataVencimento.Date < hoje).Sum(c => c.ValorTotal);
        var vencemHoje = todas.Where(c => c.Status != StatusContaFinanceira.Pago && c.DataVencimento.Date == hoje).Sum(c => c.ValorTotal);
        var aVencer = todas.Where(c => c.Status != StatusContaFinanceira.Pago && c.DataVencimento.Date > hoje).Sum(c => c.ValorTotal);
        var recebidos = noPeriodo.Where(c => c.Status == StatusContaFinanceira.Pago).Sum(c => c.ValorTotal);
        var totalPeriodo = noPeriodo.Sum(c => c.ValorTotal);

        return new ResumoContasReceber(vencidos, vencemHoje, aVencer, recebidos, totalPeriodo);
    }

    public async Task<ContaReceber> AdicionarAsync(int clienteId, string descricao, decimal valorTotal, DateTime dataVencimento)
    {
        var conta = new ContaReceber
        {
            ClienteId = clienteId,
            Descricao = descricao,
            ValorTotal = valorTotal,
            DataLancamento = DateTime.Now,
            DataVencimento = dataVencimento,
            Status = StatusContaFinanceira.Aberto
        };

        await _unitOfWork.ContasReceber.AdicionarAsync(conta);
        await _unitOfWork.SalvarAlteracoesAsync();
        return conta;
    }

    public async Task MarcarComoRecebidoAsync(int contaReceberId)
    {
        var conta = await _unitOfWork.ContasReceber.ObterPorIdAsync(contaReceberId);
        if (conta is null || conta.Status == StatusContaFinanceira.Pago)
            return;

        var pagamento = new ContaReceberPagamento
        {
            ContaReceberId = contaReceberId,
            ValorPago = conta.ValorTotal,
            DataPagamento = DateTime.Now,
            FormaPagamento = FormaPagamento.Dinheiro
        };

        await _unitOfWork.ContasReceberPagamentos.AdicionarAsync(pagamento);

        conta.Status = StatusContaFinanceira.Pago;
        _unitOfWork.ContasReceber.Atualizar(conta);

        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
