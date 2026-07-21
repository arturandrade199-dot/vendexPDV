using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Printing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Vendex.Application.Services;
using Vendex.Domain.Logging;

namespace Vendex.App.ViewModels;

public partial class ConfiguracaoBackupViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IBackupService _backupService;
    private readonly IConfiguracaoImpressaoService _configuracaoImpressaoService;
    private readonly IRelatorioProblemaService _relatorioProblemaService;

    [ObservableProperty] private bool ativo;
    [ObservableProperty] private string horarioTexto = "22:00";
    [ObservableProperty] private string? caminhoDestino;
    [ObservableProperty] private string? mensagemErro;
    [ObservableProperty] private string statusUltimoBackupFormatado = "Nenhum backup realizado ainda.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeExecutarBackup))]
    private bool executandoBackup;

    public bool PodeExecutarBackup => !ExecutandoBackup;

    public ObservableCollection<string> ImpressorasDisponiveis { get; } = new();

    [ObservableProperty] private string? impressoraSelecionada;
    [ObservableProperty] private bool imprimirAberturaCaixa = true;
    [ObservableProperty] private bool imprimirFechamentoCaixa = true;
    [ObservableProperty] private bool imprimirVenda = true;
    [ObservableProperty] private string? mensagemErroImpressao;
    [ObservableProperty] private string? mensagemSucessoImpressao;

    [ObservableProperty] private string? descricaoProblema;
    [ObservableProperty] private string? mensagemErroRelatorio;
    [ObservableProperty] private string? mensagemSucessoRelatorio;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeEnviarRelatorio))]
    private bool enviandoRelatorio;

    public bool PodeEnviarRelatorio => !EnviandoRelatorio;

    public ConfiguracaoBackupViewModel(
        IBackupService backupService, IConfiguracaoImpressaoService configuracaoImpressaoService, IRelatorioProblemaService relatorioProblemaService)
    {
        _backupService = backupService;
        _configuracaoImpressaoService = configuracaoImpressaoService;
        _relatorioProblemaService = relatorioProblemaService;
        _ = CarregarAsync();
        CarregarImpressoras();
        _ = CarregarConfiguracaoImpressaoAsync();
    }

    private async Task CarregarAsync()
    {
        var configuracao = await _backupService.ObterConfiguracaoAsync();
        Ativo = configuracao.Ativo;
        HorarioTexto = configuracao.Horario.ToString(@"hh\:mm");
        CaminhoDestino = configuracao.CaminhoDestino;
        AtualizarStatusFormatado(configuracao.UltimoBackupData, configuracao.UltimoBackupSucesso, configuracao.UltimaMensagemErro);
    }

    private void CarregarImpressoras()
    {
        // System.Printing (não System.Drawing) — mesma pilha já usada pelo PrintDialog
        // existente, sem adicionar dependência nova só pra listar as impressoras instaladas.
        using var servidor = new LocalPrintServer();
        foreach (var fila in servidor.GetPrintQueues())
            ImpressorasDisponiveis.Add(fila.FullName);
    }

    private async Task CarregarConfiguracaoImpressaoAsync()
    {
        var configuracao = await _configuracaoImpressaoService.ObterConfiguracaoAsync();
        ImpressoraSelecionada = configuracao.ImpressoraPadrao;
        ImprimirAberturaCaixa = configuracao.ImprimirAberturaCaixa;
        ImprimirFechamentoCaixa = configuracao.ImprimirFechamentoCaixa;
        ImprimirVenda = configuracao.ImprimirVenda;
    }

    [RelayCommand]
    private async Task SalvarImpressaoAsync()
    {
        MensagemErroImpressao = null;
        MensagemSucessoImpressao = null;
        try
        {
            await _configuracaoImpressaoService.SalvarConfiguracaoAsync(
                ImpressoraSelecionada, ImprimirAberturaCaixa, ImprimirFechamentoCaixa, ImprimirVenda);
            MensagemSucessoImpressao = "Preferências de impressão salvas.";
        }
        catch (Exception ex)
        {
            Logger.Error("Falha ao salvar preferências de impressão.", ex);
            MensagemErroImpressao = ex.Message;
        }
    }

    private void AtualizarStatusFormatado(DateTime? data, bool sucesso, string? mensagemErroBackup)
    {
        if (data is null)
        {
            StatusUltimoBackupFormatado = "Nenhum backup realizado ainda.";
            return;
        }

        var dataFormatada = data.Value.ToString("dd/MM/yyyy HH:mm", CulturaBr);
        StatusUltimoBackupFormatado = sucesso
            ? $"Último backup: {dataFormatada} — concluído com sucesso."
            : $"Último backup: {dataFormatada} — falhou ({mensagemErroBackup}).";
    }

    [RelayCommand]
    private void EscolherPasta()
    {
        var dialogo = new OpenFolderDialog { Title = "Escolher pasta de destino do backup" };
        if (dialogo.ShowDialog() == true)
            CaminhoDestino = dialogo.FolderName;
    }

    [RelayCommand]
    private async Task SalvarAsync() => await ValidarESalvarAsync();

    private async Task<bool> ValidarESalvarAsync()
    {
        if (!TimeSpan.TryParseExact(HorarioTexto.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var horario))
        {
            MensagemErro = "Informe o horário no formato HH:mm.";
            return false;
        }

        if (Ativo && string.IsNullOrWhiteSpace(CaminhoDestino))
        {
            MensagemErro = "Escolha uma pasta de destino antes de ativar o backup automático.";
            return false;
        }

        MensagemErro = null;
        await _backupService.SalvarConfiguracaoAsync(Ativo, horario, CaminhoDestino);
        return true;
    }

    [RelayCommand]
    private async Task BackupAgoraAsync()
    {
        if (string.IsNullOrWhiteSpace(CaminhoDestino))
        {
            MensagemErro = "Escolha uma pasta de destino antes de fazer um backup.";
            return;
        }

        // Garante que o caminho/horário digitados na tela sejam os mesmos usados no backup —
        // ExecutarBackupAsync lê a configuração persistida, não os campos em memória do ViewModel.
        if (!await ValidarESalvarAsync())
            return;

        ExecutandoBackup = true;
        try
        {
            var configuracao = await _backupService.ExecutarBackupAsync(AppPaths.PastaDados);
            AtualizarStatusFormatado(configuracao.UltimoBackupData, configuracao.UltimoBackupSucesso, configuracao.UltimaMensagemErro);
        }
        catch (Exception ex)
        {
            Logger.Error("Falha ao executar backup manual.", ex);
            MensagemErro = ex.Message;
            AtualizarStatusFormatado(DateTime.Now, false, ex.Message);
        }
        finally
        {
            ExecutandoBackup = false;
        }
    }

    [RelayCommand]
    private async Task EnviarRelatorioAsync()
    {
        MensagemErroRelatorio = null;
        MensagemSucessoRelatorio = null;

        if (string.IsNullOrWhiteSpace(DescricaoProblema))
        {
            MensagemErroRelatorio = "Descreva o problema antes de enviar.";
            return;
        }

        EnviandoRelatorio = true;
        try
        {
            var (sucesso, erro) = await _relatorioProblemaService.EnviarAsync("manual", DescricaoProblema, LerLogDeHoje());
            if (sucesso)
            {
                MensagemSucessoRelatorio = "Relatório enviado. Obrigado!";
                DescricaoProblema = null;
            }
            else
            {
                MensagemErroRelatorio = erro ?? "Não foi possível enviar o relatório.";
            }
        }
        finally
        {
            EnviandoRelatorio = false;
        }
    }

    private static string? LerLogDeHoje()
    {
        var caminho = Path.Combine(AppPaths.PastaLogs, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
        return File.Exists(caminho) ? File.ReadAllText(caminho) : null;
    }
}
