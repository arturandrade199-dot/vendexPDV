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
    // TODO: antes da primeira venda, gerar um par de chaves de produção
    // (`openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048`) e trocar a
    // chave pública abaixo. A privada correspondente vira o secret RSA_CHAVE_PRIVADA
    // no Supabase — nunca commitar a privada aqui. Esta é a chave de desenvolvimento.
    private const string ChavePublicaPem = """
        -----BEGIN PUBLIC KEY-----
        MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuHN7RxO0h4BHB20S3fxL
        YlznN3HKCToNOLeDFvvvGnhlbAM7jA41JSQcDvl/LeJbt4tkcG9bYBxnYa/gjTcz
        USDsEwE2CY7v0e4CmHR0uwr7XVxA8dHmB+7RclvvJoK2K1uTnCBUz2Jas0d+bcTb
        muxoav64vxPwl6KdNvv1XRIV25crW8joYAkX6lCHbbiDg2b3m6SCPbwTc3yVvlWK
        G7viwoLzaFYMn0ZZIe8x8XWndgX0OOxwpRmRCdbjlCqhRU87qlwic9iXOyu4PREq
        iPIUEz3qLpXujgYr2HeSx/V66GhNY7zp0DuHBbyP21l0GhkjYT4WApMMs8dG2EIy
        iQIDAQAB
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
