using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

/// <summary>Empacota a tupla de RelatoriosDisponiveis.Todos numa classe com membros
/// reais — o binding do WPF usa reflexão em tempo de execução e não enxerga nomes de
/// elementos de tupla nomeada (Item1/Item2 são os únicos membros que existem de fato).</summary>
public record OpcaoRelatorio(TipoRelatorio Tipo, string Nome, bool PrecisaPeriodo);
