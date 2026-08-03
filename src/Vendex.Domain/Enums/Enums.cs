namespace Vendex.Domain.Enums;

public enum TipoUsuario
{
    Administrador,
    Funcionario
}

public enum StatusContaFinanceira
{
    Aberto,
    Parcial,
    Pago,
    Atrasado
}

public enum FormaPagamento
{
    Dinheiro,
    CartaoCredito,
    CartaoDebito,
    Pix,
    Beneficios,
    /// <summary>Venda a prazo — gera um lançamento em ContaReceber vinculado ao cliente.</summary>
    Fiado
}

public enum StatusCaixa
{
    Aberto,
    Fechado
}

public enum TipoMovimentacaoCaixa
{
    Sangria,
    Reforco
}

public enum StatusLicenca
{
    NaoAtivado,
    Ativo,
    Bloqueado
}

public enum TipoAcaoAuditoria
{
    Inclusao,
    Alteracao,
    Exclusao,
    Login,
    Logout,
    CancelamentoVenda,
    AberturaCaixa,
    FechamentoCaixa,
    TentativaAcessoNegado,
    RegistroDevolucao
}

public enum SituacaoLote
{
    Valido,
    Vencendo,
    Vencido,
    Esgotado
}

public enum UnidadeMedida
{
    UN,
    KG,
    G,
    L,
    ML,
    CX,
    PC,
    PAR,
    DZ,
    M,
    M2,
    M3,
    RL,
    FD,
    SC,
    KIT
}
