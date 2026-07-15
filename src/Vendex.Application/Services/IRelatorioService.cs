namespace Vendex.Application.Services;

public interface IRelatorioService
{
    Task<RelatorioResultado> GerarAsync(TipoRelatorio tipo, DateTime? inicio, DateTime? fim);
    byte[] ExportarPdf(RelatorioResultado resultado);
    byte[] ExportarExcel(RelatorioResultado resultado);
}
