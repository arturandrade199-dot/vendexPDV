// Check-in diário: o app confere se a assinatura continua ativa e recebe mais 7 dias
// de validade assinada, se sim. Devolve sempre uma resposta ASSINADA (ativo:true ou
// ativo:false) em vez de um erro HTTP genérico quando a assinatura não está mais
// ativa — assim o app do lado do cliente consegue distinguir "servidor confirmou que
// não está mais ativo" (bloqueia) de "sem internet/erro de rede" (usa a folga offline
// dos últimos 7 dias já confirmados, ver Vendex.Application.LicencaService).
import { assinarPayload } from "../_shared/assinatura.ts";
import { criarClienteAdmin, respostaJson } from "../_shared/supabaseAdmin.ts";

Deno.serve(async (req) => {
  if (req.method !== "POST") return respostaJson({ erro: "método não permitido" }, 405);

  const { email, fingerprint } = await req.json().catch(() => ({}));
  if (!email || !fingerprint) return respostaJson({ erro: "informe email e fingerprint" }, 400);

  const emailNormalizado = String(email).toLowerCase().trim();
  const supabase = criarClienteAdmin();

  const { data: linha, error } = await supabase
    .from("licencas_assinatura")
    .select("*")
    .eq("email", emailNormalizado)
    .maybeSingle();

  if (error) {
    console.error(error);
    return respostaJson({ erro: "falha ao consultar assinatura" }, 500);
  }

  const ativo = !!linha && linha.status === "ativo" && linha.fingerprint === fingerprint;

  const validoAte = new Date();
  if (ativo) validoAte.setDate(validoAte.getDate() + 7);

  const resultado = await assinarPayload({
    fingerprint,
    ativo,
    validoAte: validoAte.toISOString(),
    emitidoEm: new Date().toISOString(),
  });

  return respostaJson(resultado);
});
