namespace Vendex.Domain.Logging;

/// <summary>Log local em arquivo texto, um por dia, na pasta configurada via <see cref="Configure"/>
/// (normalmente "dados/logs" ao lado do banco). Sem dependência de biblioteca externa: o objetivo é
/// dar visibilidade a erros que hoje somem sem deixar rastro, não construir infraestrutura de
/// observabilidade completa.</summary>
public static class Logger
{
    private static readonly object Trava = new();
    private static string? _pastaLogs;

    public static void Configure(string pastaLogs)
    {
        _pastaLogs = pastaLogs;
        Directory.CreateDirectory(pastaLogs);
    }

    public static void Info(string mensagem) => Escrever("INFO", mensagem, null);

    public static void Warn(string mensagem, Exception? excecao = null) => Escrever("AVISO", mensagem, excecao);

    public static void Error(string mensagem, Exception excecao) => Escrever("ERRO", mensagem, excecao);

    private static void Escrever(string nivel, string mensagem, Exception? excecao)
    {
        if (_pastaLogs is null)
            return;

        try
        {
            var linha = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{nivel}] {mensagem}";
            if (excecao is not null)
                linha += Environment.NewLine + excecao;

            var caminhoArquivo = Path.Combine(_pastaLogs, $"log-{DateTime.Now:yyyy-MM-dd}.txt");
            lock (Trava)
                File.AppendAllText(caminhoArquivo, linha + Environment.NewLine + Environment.NewLine);
        }
        catch
        {
            // Falha ao gravar o log não pode gerar outra exceção nem derrubar o app —
            // é o último recurso, não há mais nada abaixo dele pra registrar essa falha.
        }
    }
}
