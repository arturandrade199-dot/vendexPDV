using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class DevolucaoService : IDevolucaoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICaixaService _caixaService;

    public DevolucaoService(IUnitOfWork unitOfWork, ICaixaService caixaService)
    {
        _unitOfWork = unitOfWork;
        _caixaService = caixaService;
    }

    public async Task<Devolucao> RegistrarAsync(
        int? clienteId, int? vendaId, int usuarioId, string? motivo,
        IReadOnlyList<ItemDevolucao> itens, bool estornarCaixa)
    {
        if (itens.Count == 0)
            throw new InvalidOperationException("Adicione ao menos um item para devolver.");

        // Confere o caixa aberto ANTES de mexer em estoque/persistir a devolução — assim,
        // se estornarCaixa for pedido sem caixa aberto, nada fica salvo pela metade.
        if (estornarCaixa && await _caixaService.ObterCaixaAbertoAsync() is null)
            throw new InvalidOperationException("Não há caixa aberto para estornar o valor.");

        var valorTotal = itens.Sum(i => i.Quantidade * i.PrecoUnitario);

        var devolucao = new Devolucao
        {
            ClienteId = clienteId,
            VendaId = vendaId,
            UsuarioId = usuarioId,
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim(),
            ValorTotal = valorTotal,
            DataHora = DateTime.Now,
            EstornouCaixa = estornarCaixa
        };

        foreach (var item in itens)
        {
            devolucao.Itens.Add(new DevolucaoItem
            {
                ProdutoId = item.ProdutoId,
                ProdutoVarianteId = item.ProdutoVarianteId,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                Subtotal = item.Quantidade * item.PrecoUnitario
            });

            // Espelho do decremento em VendaService.FinalizarVendaAsync — mesma ramificação
            // entre produto simples e variante, só que somando em vez de subtraindo.
            var produto = await _unitOfWork.Produtos.ObterPorIdAsync(item.ProdutoId);
            if (produto is not null)
            {
                if (item.ProdutoVarianteId is int varianteId)
                {
                    var variante = produto.Variantes.FirstOrDefault(v => v.Id == varianteId);
                    if (variante is not null)
                        variante.EstoqueAtual += item.Quantidade;
                }
                else
                {
                    produto.EstoqueAtual += item.Quantidade;
                }

                _unitOfWork.Produtos.Atualizar(produto);
            }
        }

        await _unitOfWork.Devolucoes.AdicionarAsync(devolucao);
        await _unitOfWork.SalvarAlteracoesAsync();

        if (estornarCaixa)
        {
            var referenciaVenda = vendaId is int v ? $" — Venda #{v}" : string.Empty;
            await _caixaService.RegistrarMovimentacaoAsync(
                usuarioId, TipoMovimentacaoCaixa.Sangria, valorTotal, $"Devolução de mercadoria{referenciaVenda}");
        }

        return devolucao;
    }
}
