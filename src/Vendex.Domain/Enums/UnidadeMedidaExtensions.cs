namespace Vendex.Domain.Enums;

public static class UnidadeMedidaExtensions
{
    public static string ParaTexto(this UnidadeMedida unidade) => unidade switch
    {
        UnidadeMedida.UN => "UN — Unidade",
        UnidadeMedida.KG => "KG — Quilograma",
        UnidadeMedida.G => "G — Grama",
        UnidadeMedida.L => "L — Litro",
        UnidadeMedida.ML => "ML — Mililitro",
        UnidadeMedida.CX => "CX — Caixa",
        UnidadeMedida.PC => "PC — Pacote",
        UnidadeMedida.PAR => "PAR — Par",
        UnidadeMedida.DZ => "DZ — Dúzia",
        UnidadeMedida.M => "M — Metro",
        UnidadeMedida.M2 => "M2 — Metro quadrado",
        UnidadeMedida.M3 => "M3 — Metro cúbico",
        UnidadeMedida.RL => "RL — Rolo",
        UnidadeMedida.FD => "FD — Fardo",
        UnidadeMedida.SC => "SC — Saco",
        UnidadeMedida.KIT => "KIT — Kit",
        _ => unidade.ToString()
    };
}
