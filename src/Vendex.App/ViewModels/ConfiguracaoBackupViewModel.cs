using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class ConfiguracaoBackupViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IBackupService _backupService;

    [ObservableProperty] private bool ativo;
    [ObservableProperty] private string horarioTexto = "22:00";
    [ObservableProperty] private string? caminhoDestino;
    [ObservableProperty] private string? mensagemErro;
    [ObservableProperty] private string statusUltimoBackupFormatado = "Nenhum backup realizado ainda.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeExecutarBackup))]
    private bool executandoBackup;

    public bool PodeExecutarBackup => !ExecutandoBackup;

    public ConfiguracaoBackupViewModel(IBackupService backupService)
    {
        _backupService = backupService;
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var configuracao = await _backupService.ObterConfiguracaoAsync();
        Ativo = configuracao.Ativo;
        HorarioTexto = configuracao.Horario.ToString(@"hh\:mm");
        CaminhoDestino = configuracao.CaminhoDestino;
        AtualizarStatusFormatado(configuracao.UltimoBackupData, configuracao.UltimoBackupSucesso, configuracao.UltimaMensagemErro);
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
            MensagemErro = ex.Message;
            AtualizarStatusFormatado(DateTime.Now, false, ex.Message);
        }
        finally
        {
            ExecutandoBackup = false;
        }
    }
}
