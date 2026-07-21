// Envio de email via Resend (https://resend.com) — API simples o bastante pra chamar
// direto com fetch, sem SDK. Exige domínio verificado no Resend pra RESEND_REMETENTE
// (o endereço de teste onboarding@resend.dev só entrega pro dono da conta).
export async function enviarEmail(assunto: string, corpo: string): Promise<boolean> {
  const chave = Deno.env.get("RESEND_API_KEY");
  const remetente = Deno.env.get("EMAIL_REMETENTE");
  const destinatario = Deno.env.get("EMAIL_DESTINATARIO");

  if (!chave || !remetente || !destinatario) {
    console.error("Email não configurado (faltam os secrets RESEND_API_KEY/EMAIL_REMETENTE/EMAIL_DESTINATARIO).");
    return false;
  }

  const resposta = await fetch("https://api.resend.com/emails", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${chave}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      from: remetente,
      to: destinatario,
      subject: assunto,
      text: corpo,
    }),
  });

  if (!resposta.ok) {
    console.error("Falha ao enviar email:", await resposta.text());
    return false;
  }

  return true;
}
