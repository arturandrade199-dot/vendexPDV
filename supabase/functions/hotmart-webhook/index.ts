// Recebe o postback/webhook da Hotmart e mantém licencas_assinatura em dia.
//
// ATENÇÃO — verificar antes de ir pra produção: os nomes exatos de campo abaixo
// (data.buyer.email, data.purchase.status, data.subscription.status) foram
// confirmados pela documentação pública da Hotmart (developers.hotmart.com,
// webhook 2.0) na época em que este arquivo foi escrito, mas a Hotmart já mudou esse
// formato antes. Configure o webhook no painel da Hotmart, dispare uma compra de
// teste, e confira no log desta function (`supabase functions logs hotmart-webhook`)
// se os campos abaixo batem com o payload real recebido — ajuste se não bater.
import { criarClienteAdmin, respostaJson } from "../_shared/supabaseAdmin.ts";

const HOTTOK_ESPERADO = Deno.env.get("HOTMART_HOTTOK");

// Eventos que liberam acesso (compra aprovada ou recorrência cobrada com sucesso).
const EVENTOS_ATIVAR = new Set(["PURCHASE_APPROVED", "PURCHASE_COMPLETE"]);

// Eventos que bloqueiam acesso (cancelamento, reembolso, chargeback, atraso).
const EVENTOS_CANCELAR = new Set([
  "PURCHASE_CANCELED",
  "PURCHASE_REFUNDED",
  "PURCHASE_CHARGEBACK",
  "SUBSCRIPTION_CANCELLATION",
  "PURCHASE_EXPIRED",
]);

const EVENTOS_ATRASO = new Set(["PURCHASE_DELAYED", "PURCHASE_BILLET_PRINTED"]);

Deno.serve(async (req) => {
  if (req.method !== "POST") return respostaJson({ erro: "método não permitido" }, 405);

  const payload = await req.json();

  // A Hotmart manda o token tanto no corpo (payload.hottok) quanto, em algumas
  // versões do webhook, no header X-Hotmart-Hottok — aceitando os dois evita
  // depender de qual formato exato a conta do usuário está configurada pra usar.
  const hottokRecebido = payload?.hottok ?? req.headers.get("x-hotmart-hottok");
  if (!HOTTOK_ESPERADO || hottokRecebido !== HOTTOK_ESPERADO) {
    return respostaJson({ erro: "hottok inválido" }, 401);
  }

  const evento: string | undefined = payload?.event;
  const email: string | undefined = payload?.data?.buyer?.email;
  const nome: string | undefined = payload?.data?.buyer?.name;
  const assinaturaId: string | undefined = payload?.data?.subscription?.subscriber?.code
    ?? payload?.data?.purchase?.transaction;

  if (!evento || !email) {
    return respostaJson({ erro: "payload sem event/buyer.email" }, 400);
  }

  let novoStatus: string | null = null;
  if (EVENTOS_ATIVAR.has(evento)) novoStatus = "ativo";
  else if (EVENTOS_CANCELAR.has(evento)) novoStatus = "cancelado";
  else if (EVENTOS_ATRASO.has(evento)) novoStatus = "atrasado";

  if (novoStatus === null) {
    // Evento que não muda o status de acesso (ex: PIX gerado mas não pago ainda) —
    // confirma recebimento sem alterar nada, pra Hotmart não ficar retentando.
    return respostaJson({ ok: true, ignorado: evento });
  }

  const supabase = criarClienteAdmin();
  const agora = new Date().toISOString();

  const { error } = await supabase
    .from("licencas_assinatura")
    .upsert(
      {
        email: email.toLowerCase().trim(),
        nome: nome ?? null,
        status: novoStatus,
        hotmart_assinatura_id: assinaturaId ?? null,
        data_ultima_cobranca: novoStatus === "ativo" ? agora : undefined,
        atualizado_em: agora,
      },
      { onConflict: "email" },
    );

  if (error) {
    console.error("Erro ao gravar licencas_assinatura:", error);
    return respostaJson({ erro: "falha ao gravar assinatura" }, 500);
  }

  return respostaJson({ ok: true, email, status: novoStatus });
});
