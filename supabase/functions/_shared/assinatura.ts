// Assina payloads com a chave RSA privada (secret RSA_CHAVE_PRIVADA, formato PEM PKCS8
// — o mesmo gerado por `openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048`).
// RSASSA-PKCS1-v1_5 + SHA-256, compatível com
// RSA.VerifyData(dados, assinatura, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
// no lado C# (Vendex.Licensing.AssinaturaVerifier) — testado de ponta a ponta antes de
// escrever este arquivo (Node assina, .NET confere, payload adulterado é rejeitado).

function pemParaArrayBuffer(pem: string): ArrayBuffer {
  const base64 = pem
    .replace(/-----BEGIN PRIVATE KEY-----/, "")
    .replace(/-----END PRIVATE KEY-----/, "")
    .replace(/\s/g, "");
  const binario = atob(base64);
  const bytes = new Uint8Array(binario.length);
  for (let i = 0; i < binario.length; i++) bytes[i] = binario.charCodeAt(i);
  return bytes.buffer;
}

export interface RespostaAssinada {
  payload: string;
  assinatura: string;
}

export async function assinarPayload(payloadObjeto: unknown): Promise<RespostaAssinada> {
  const chavePem = Deno.env.get("RSA_CHAVE_PRIVADA");
  if (!chavePem) throw new Error("Secret RSA_CHAVE_PRIVADA não configurada no projeto Supabase.");

  const chave = await crypto.subtle.importKey(
    "pkcs8",
    pemParaArrayBuffer(chavePem),
    { name: "RSASSA-PKCS1-v1_5", hash: "SHA-256" },
    false,
    ["sign"],
  );

  const payload = JSON.stringify(payloadObjeto);
  const assinaturaBuffer = await crypto.subtle.sign(
    "RSASSA-PKCS1-v1_5",
    chave,
    new TextEncoder().encode(payload),
  );
  const assinatura = btoa(String.fromCharCode(...new Uint8Array(assinaturaBuffer)));

  return { payload, assinatura };
}
