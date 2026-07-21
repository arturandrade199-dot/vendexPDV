using System.Security.Cryptography;
using System.Text;

namespace Vendex.Licensing;

/// <summary>
/// Confere a assinatura RSA emitida pelas Edge Functions do Supabase
/// (ativar-licenca/verificar-licenca) — RSASSA-PKCS1-v1_5 + SHA-256, testado de ponta
/// a ponta contra a implementação em TypeScript (Web Crypto) antes de escrever esta
/// classe. Só a chave PÚBLICA fica aqui: a privada nunca sai do Supabase, então
/// ninguém consegue forjar uma licença mesmo extraindo tudo deste .exe.
/// </summary>
public static class AssinaturaVerifier
{
    // Chave pública de PRODUÇÃO (par gerado em 2026-07-20). A privada correspondente
    // foi cadastrada como secret RSA_CHAVE_PRIVADA no Supabase e nunca é commitada.
    private const string ChavePublicaPem = """
        -----BEGIN PUBLIC KEY-----
        MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApu5rdYcYJcF6j7C5UXEk
        FKw/vJWUnOP+CEhMlB8gXNCaVWHZ4SGGgOfgur2Zm3xiurU/1oR+273qdHug2EQg
        3svEWPYarf65ikr0gDRKdOioLr6F2lUEQSuVPnWRX+CC1kyrEHyHQ+kwT0jJrU5S
        ISVeSpf4V+5y3kDuWdcx5b1C9EQPf3tHOLk27C66WujXNq8tDzny4ZGw6pmi91p+
        bmyqwsMm0uZukZCuglRgCtM2kzYlMCBr7fcA9GSQFx67bDisjOLng7bpJJ9BR0SV
        TK19L5IdW4Zti+Y9i6vLG7lGFqbPZ6w5ZFZeJdbEJraUzO/BY/vxL36hSeY2/8tM
        xwIDAQAB
        -----END PUBLIC KEY-----
        """;

    /// <summary>Confere se <paramref name="assinaturaBase64"/> é uma assinatura RSA
    /// válida (chave privada do Supabase) para o texto exato de <paramref name="payloadJson"/>.</summary>
    public static bool VerificarAssinatura(string payloadJson, string assinaturaBase64)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(ChavePublicaPem);

            var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
            var assinaturaBytes = Convert.FromBase64String(assinaturaBase64);

            return rsa.VerifyData(payloadBytes, assinaturaBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}
