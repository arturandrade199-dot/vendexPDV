using System.Security.Cryptography;
using System.Text;

namespace Vendex.Licensing;

public static class SerialAlgorithm
{
    // TODO: antes da primeira venda, gerar uma chave própria e versionar fora do
    // controle de código-fonte (variável de ambiente / arquivo protegido), igual ao
    // chave.dat do GeradorSerial em Delphi. Esta chave é só para desenvolvimento.
    private const string ChavePrivada = "VENDEX-DEV-0000000000000000000000000000";

    /// <summary>
    /// Gera o serial de ativação esperado para um código de instalação, no formato
    /// XXXX-XXXX-XXXX-XXXX-XXXX. Determinístico: o mesmo código de instalação sempre
    /// produz o mesmo serial, usado tanto no GeradorSerial (vendedor) quanto na
    /// validação dentro do Vendex.App (cliente).
    /// </summary>
    public static string GerarSerial(string codigoInstalacao)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ChavePrivada));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(codigoInstalacao.ToUpperInvariant()));
        var hex = Convert.ToHexString(hash)[..20];

        return string.Join('-', Enumerable.Range(0, 5).Select(i => hex.Substring(i * 4, 4)));
    }

    public static bool ValidarSerial(string codigoInstalacao, string serialInformado)
    {
        var esperado = GerarSerial(codigoInstalacao);
        return string.Equals(esperado, serialInformado?.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
