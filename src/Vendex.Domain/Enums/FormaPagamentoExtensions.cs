namespace Vendex.Domain.Enums;

public static class FormaPagamentoExtensions
{
    public static string ParaTexto(this FormaPagamento forma) => forma switch
    {
        FormaPagamento.Dinheiro => "Dinheiro",
        FormaPagamento.CartaoCredito => "Cartão de Crédito",
        FormaPagamento.CartaoDebito => "Cartão de Débito",
        FormaPagamento.Pix => "Pix",
        FormaPagamento.Beneficios => "Benefícios",
        FormaPagamento.Fiado => "Fiado",
        _ => forma.ToString()
    };
}
