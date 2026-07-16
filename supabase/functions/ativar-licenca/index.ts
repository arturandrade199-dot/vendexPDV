// Primeira ativação: o app manda o email usado na compra + o fingerprint da máquina.
// Se a assinatura estiver ativa e sem máquina vinculada ainda, vincula e devolve uma
// licença assinada (7 dias de validade, renovada depois pelo check-in diário de
// verificar-licenca). 1 máquina por assinatura — trocar de máquina é uma exceção
// manual (o vendedor limpa o campo "fingerprint" da linha direto no painel do Supabase).
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

  if (!linha || linha.status !== "ativo") {
    return respostaJson(
      { erro: "email não encontrado ou assinatura não está ativa — confirme sua compra na Hotmart" },
      404,
    );
  }

  if (linha.fingerprint && linha.fingerprint !== fingerprint) {
    return respostaJson({ erro: "esta assinatura já está ativada em outra máquina" }, 409);
  }

  if (!linha.fingerprint) {
    const { error: erroUpdate } = await supabase
      .from("licencas_assinatura")
      .update({ fingerprint, atualizado_em: new Date().toISOString() })
      .eq("id", linha.id);

    if (erroUpdate) {
      console.error(erroUpdate);
      return respostaJson({ erro: "falha ao vincular esta máquina à assinatura" }, 500);
    }
  }

  const validoAte = new Date();
  validoAte.setDate(validoAte.getDate() + 7);

  const resultado = await assinarPayload({
    fingerprint,
    ativo: true,
    validoAte: validoAte.toISOString(),
    emitidoEm: new Date().toISOString(),
  });

  return respostaJson(resultado);
});
