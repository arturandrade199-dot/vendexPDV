using System.Management;
using System.Security.Cryptography;
using System.Text;
using Vendex.Domain.Logging;

namespace Vendex.Licensing;

public static class FingerprintProvider
{
    /// <summary>
    /// Gera o código de instalação a partir de identificadores de hardware da máquina
    /// (serial do disco de boot + UUID da placa-mãe), no formato XXXX-XXXX-XXXX-XXXX.
    /// </summary>
    public static string ObterCodigoInstalacao()
    {
        var serialDisco = ObterPropriedadeWmi("Win32_DiskDrive", "SerialNumber");
        var uuidPlaca = ObterPropriedadeWmi("Win32_ComputerSystemProduct", "UUID");

        var bruto = $"{serialDisco}|{uuidPlaca}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(bruto));
        var hex = Convert.ToHexString(hash)[..16];

        return string.Join('-', Enumerable.Range(0, 4).Select(i => hex.Substring(i * 4, 4)));
    }

    private static string ObterPropriedadeWmi(string classe, string propriedade)
    {
        try
        {
            using var pesquisador = new ManagementObjectSearcher($"SELECT {propriedade} FROM {classe}");
            foreach (var objeto in pesquisador.Get())
            {
                var valor = objeto[propriedade]?.ToString();
                if (!string.IsNullOrWhiteSpace(valor))
                    return valor.Trim();
            }
        }
        catch (ManagementException ex)
        {
            // Máquina sem WMI disponível (raro) — cai no valor fixo abaixo.
            Logger.Warn($"WMI indisponível ao ler {propriedade} de {classe}.", ex);
        }

        return "INDISPONIVEL";
    }
}
