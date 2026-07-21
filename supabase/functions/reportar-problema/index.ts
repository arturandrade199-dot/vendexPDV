// Recebe relatórios de problema do app desktop — tanto o envio automático diário (quando
// o log do dia anterior teve ERRO/AVISO) quanto o form manual de "reportar um problema".
// Sempre grava em relatorios_problema (histórico consultável direto no Supabase, mesmo se
// o email falhar) e tenta notificar por email best-effort.
import { criarClienteAdmin, respostaJson } from "../_shared/supabaseAdmin.ts";
import { enviarEmail } from "../_shared/email.ts";

const TAMANHO_MAXIMO_MENSAGEM = 5_000;
const TAMANHO_MAXIMO_LOG = 20_000;
const JANELA_LIMITE_MINUTOS = 5;

Deno.serve(async (req) => {
  if (req.method !== "POST") return respostaJson({ erro: "método não permitido" }, 405);

  const { fingerprint, email, tipo, mensagem, log } = await req.json().catch(() => ({}));

  if (!fingerprint || typeof fingerprint !== "string") {
    return respostaJson({ erro: "informe o fingerprint" }, 400);
  }
  if (tipo !== "automatico" && tipo !== "manual") {
    return respostaJson({ erro: "tipo precisa ser 'automatico' ou 'manual'" }, 400);
  }
  if (!mensagem && !log) {
    return respostaJson({ erro: "informe mensagem ou log" }, 400);
  }

  const mensagemTruncada = typeof mensagem === "string" ? mensagem.slice(0, TAMANHO_MAXIMO_MENSAGEM) : null;
  const logTruncado = typeof log === "string" ? log.slice(0, TAMANHO_MAXIMO_LOG) : null;

  const supabase = criarClienteAdmin();

  // Limite simples por fingerprint: evita que um erro em loop (ou alguém batendo direto
  // na function, já que a URL vem embutida no .exe) vire uma enxurrada de emails.
  const { data: ultimoRelatorio, error: erroConsulta } = await supabase
    .from("relatorios_problema")
    .select("criado_em")
    .eq("fingerprint", fingerprint)
    .order("criado_em", { ascending: false })
    .limit(1)
    .maybeSingle();

  if (erroConsulta) {
    console.error(erroConsulta);
    return respostaJson({ erro: "falha ao consultar relatórios anteriores" }, 500);
  }

  if (ultimoRelatorio) {
    const minutosDesdeUltimo = (Date.now() - new Date(ultimoRelatorio.criado_em).getTime()) / 60_000;
    if (minutosDesdeUltimo < JANELA_LIMITE_MINUTOS) {
      return respostaJson({ erro: "aguarde alguns minutos antes de enviar outro relatório" }, 429);
    }
  }

  const { error: erroInsercao } = await supabase.from("relatorios_problema").insert({
    fingerprint,
    email: typeof email === "string" ? email : null,
    tipo,
    mensagem: mensagemTruncada,
    log: logTruncado,
  });

  if (erroInsercao) {
    console.error(erroInsercao);
    return respostaJson({ erro: "falha ao registrar relatório" }, 500);
  }

  const assunto = tipo === "automatico"
    ? `[Vendex PDV] Relatório automático de erros — ${fingerprint}`
    : `[Vendex PDV] Problema relatado por usuário — ${fingerprint}`;

  const partesCorpo = [
    `Fingerprint: ${fingerprint}`,
    email ? `Email: ${email}` : null,
    mensagemTruncada ? `Mensagem do usuário:\n${mensagemTruncada}` : null,
    logTruncado ? `Log:\n${logTruncado}` : null,
  ].filter((parte): parte is string => parte !== null);

  const emailEnviado = await enviarEmail(assunto, partesCorpo.join("\n\n"));
  if (!emailEnviado) {
    console.error("Relatório salvo no banco, mas o email não foi enviado — confira os secrets de email.");
  }

  return respostaJson({ sucesso: true });
});
