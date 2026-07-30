using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class RelatoriosViewModel : ObservableObject
{
    private readonly IRelatorioService _relatorioService;

    public IReadOnlyList<OpcaoRelatorio> Opcoes { get; } = RelatoriosDisponiveis.Todos
        .Select(t => new OpcaoRelatorio(t.Tipo, t.Nome, t.PrecisaPeriodo))
        .ToList();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrecisaPeriodo))]
    private OpcaoRelatorio opcaoSelecionada;

    [ObservableProperty] private DateTime? dataInicio = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime? dataFim = DateTime.Today;
    [ObservableProperty] private string? mensagemErro;
    [ObservableProperty] private bool gerando;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemResultado))]
    [NotifyPropertyChangedFor(nameof(TotaisFormatados))]
    private RelatorioResultado? resultado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeExportar))]
    private bool exportando;

    public bool PrecisaPeriodo => OpcaoSelecionada.PrecisaPeriodo;
    public bool TemResultado => Resultado is not null;
    public bool PodeExportar => !Exportando;

    public string? TotaisFormatados => Resultado?.Totais is { Count: > 0 } totais
        ? string.Join("      ", totais.Select(t => $"{t.Rotulo}: {t.Valor}"))
        : null;

    public RelatoriosViewModel(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
        opcaoSelecionada = Opcoes[0];
    }

    partial void OnOpcaoSelecionadaChanged(OpcaoRelatorio value)
    {
        // "Contas a receber/pagar no período" filtram pela data de VENCIMENTO, que pra contas
        // em aberto normalmente está no futuro (ex.: venda fiado vence em 30 dias). O padrão
        // "últimos 30 dias até hoje" usado pelos relatórios históricos nunca encontra nada
        // aqui, então esses dois usam uma janela bem mais larga por padrão.
        if (value.Tipo is TipoRelatorio.ContasAReceber or TipoRelatorio.ContasAPagar)
        {
            DataInicio = DateTime.Today.AddDays(-180);
            DataFim = DateTime.Today.AddDays(180);
        }
        else
        {
            DataInicio = DateTime.Today.AddDays(-30);
            DataFim = DateTime.Today;
        }
    }

    [RelayCommand]
    private async Task GerarAsync()
    {
        MensagemErro = null;
        Gerando = true;
        try
        {
            Resultado = await _relatorioService.GerarAsync(OpcaoSelecionada.Tipo, DataInicio, DataFim);
        }
        catch (InvalidOperationException ex)
        {
            MensagemErro = ex.Message;
            Resultado = null;
        }
        finally
        {
            Gerando = false;
        }
    }

    [RelayCommand]
    private async Task ExportarPdfAsync()
    {
        if (Resultado is null)
            return;

        var dialogo = new SaveFileDialog { FileName = $"{Resultado.Titulo}.pdf", Filter = "PDF (*.pdf)|*.pdf" };
        if (dialogo.ShowDialog() != true)
            return;

        var resultado = Resultado;
        var caminho = dialogo.FileName;
        Exportando = true;
        try
        {
            // QuestPDF + gravação em disco são síncronos e podem levar um tempo perceptível
            // em relatórios grandes — sem o Task.Run, isso congelaria a tela enquanto roda.
            await Task.Run(() => File.WriteAllBytes(caminho, _relatorioService.ExportarPdf(resultado)));
        }
        finally
        {
            Exportando = false;
        }
    }

    [RelayCommand]
    private async Task ExportarExcelAsync()
    {
        if (Resultado is null)
            return;

        var dialogo = new SaveFileDialog { FileName = $"{Resultado.Titulo}.xlsx", Filter = "Excel (*.xlsx)|*.xlsx" };
        if (dialogo.ShowDialog() != true)
            return;

        var resultado = Resultado;
        var caminho = dialogo.FileName;
        Exportando = true;
        try
        {
            await Task.Run(() => File.WriteAllBytes(caminho, _relatorioService.ExportarExcel(resultado)));
        }
        finally
        {
            Exportando = false;
        }
    }
}
