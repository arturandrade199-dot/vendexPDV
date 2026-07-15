using System.IO.Compression;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class BackupService : IBackupService
{
    private const int QuantidadeBackupsRetidos = 30;

    private readonly IUnitOfWork _unitOfWork;

    public BackupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ConfiguracaoBackup> ObterConfiguracaoAsync()
    {
        // ObterPorIdAsync (rastreado) em vez de ObterTodosAsync (AsNoTracking) — o DbContext é
        // singleton na aplicação real, então uma leitura sem tracking cria uma segunda instância
        // com o mesmo Id da já rastreada por uma chamada anterior, e o EF recusa rastrear as
        // duas ao salvar (mesmo problema já visto em UsuarioService/SessaoUsuario).
        var configuracao = await _unitOfWork.ConfiguracoesBackup.ObterPorIdAsync(1);
        if (configuracao is not null)
            return configuracao;

        configuracao = new ConfiguracaoBackup();
        await _unitOfWork.ConfiguracoesBackup.AdicionarAsync(configuracao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return configuracao;
    }

    public async Task SalvarConfiguracaoAsync(bool ativo, TimeSpan horario, string? caminhoDestino)
    {
        var configuracao = await ObterConfiguracaoAsync();
        configuracao.Ativo = ativo;
        configuracao.Horario = horario;
        configuracao.CaminhoDestino = caminhoDestino;

        _unitOfWork.ConfiguracoesBackup.Atualizar(configuracao);
        await _unitOfWork.SalvarAlteracoesAsync();
    }

    public async Task<ConfiguracaoBackup> ExecutarBackupAsync(string caminhoPastaDados)
    {
        var configuracao = await ObterConfiguracaoAsync();

        try
        {
            if (string.IsNullOrWhiteSpace(configuracao.CaminhoDestino))
                throw new InvalidOperationException("Configure o caminho de destino antes de fazer um backup.");

            var pastaTemporaria = Path.Combine(Path.GetTempPath(), $"vendex-backup-{Guid.NewGuid()}");
            Directory.CreateDirectory(pastaTemporaria);

            try
            {
                // Só a cópia do banco toca o DbContext (compartilhado, não thread-safe) — fica
                // na thread de quem chamou. O resto (copiar fotos, zipar, apagar temporário,
                // retenção) é I/O de arquivo puro e pode ser mais lento com pastas grandes; jogar
                // pra uma thread do pool evita travar a tela enquanto isso roda.
                await _unitOfWork.BackupBancoDadosAsync(Path.Combine(pastaTemporaria, "vendex.db"));

                var caminhoDestino = configuracao.CaminhoDestino;
                await Task.Run(() =>
                {
                    var pastaFotosOrigem = Path.Combine(caminhoPastaDados, "fotos");
                    if (Directory.Exists(pastaFotosOrigem))
                        CopiarPasta(pastaFotosOrigem, Path.Combine(pastaTemporaria, "fotos"));

                    Directory.CreateDirectory(caminhoDestino);
                    var nomeArquivo = $"vendex-backup-{DateTime.Now:yyyyMMdd-HHmmssfff}.zip";
                    var caminhoArquivoZip = Path.Combine(caminhoDestino, nomeArquivo);
                    ZipFile.CreateFromDirectory(pastaTemporaria, caminhoArquivoZip);

                    AplicarRetencao(caminhoDestino);
                });
            }
            finally
            {
                await Task.Run(() => Directory.Delete(pastaTemporaria, recursive: true));
            }

            configuracao.UltimoBackupData = DateTime.Now;
            configuracao.UltimoBackupSucesso = true;
            configuracao.UltimaMensagemErro = null;
        }
        catch (Exception ex)
        {
            configuracao.UltimoBackupData = DateTime.Now;
            configuracao.UltimoBackupSucesso = false;
            configuracao.UltimaMensagemErro = ex.Message;

            _unitOfWork.ConfiguracoesBackup.Atualizar(configuracao);
            await _unitOfWork.SalvarAlteracoesAsync();
            throw;
        }

        _unitOfWork.ConfiguracoesBackup.Atualizar(configuracao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return configuracao;
    }

    private static void CopiarPasta(string origem, string destino)
    {
        Directory.CreateDirectory(destino);
        foreach (var arquivo in Directory.GetFiles(origem))
            File.Copy(arquivo, Path.Combine(destino, Path.GetFileName(arquivo)));

        foreach (var subPasta in Directory.GetDirectories(origem))
            CopiarPasta(subPasta, Path.Combine(destino, Path.GetFileName(subPasta)));
    }

    private static void AplicarRetencao(string caminhoDestino)
    {
        var arquivos = Directory.GetFiles(caminhoDestino, "vendex-backup-*.zip")
            .OrderByDescending(a => a)
            .Skip(QuantidadeBackupsRetidos);

        foreach (var arquivo in arquivos)
            File.Delete(arquivo);
    }
}
