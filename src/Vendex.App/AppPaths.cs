using System.IO;

namespace Vendex.App;

internal static class AppPaths
{
    public static string PastaDados { get; } = Path.Combine(AppContext.BaseDirectory, "dados");
    public static string PastaFotos { get; } = Path.Combine(PastaDados, "fotos");
    public static string CaminhoBanco { get; } = Path.Combine(PastaDados, "vendex.db");
    public static string PastaLogs { get; } = Path.Combine(PastaDados, "logs");
}
