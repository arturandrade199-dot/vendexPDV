import { createClient } from "https://esm.sh/@supabase/supabase-js@2";

// SUPABASE_URL e SUPABASE_SERVICE_ROLE_KEY são injetadas automaticamente pela
// plataforma em toda Edge Function — não precisam ser cadastradas como secret manual.
// A service_role key ignora RLS, por isso essas functions conseguem ler/escrever
// licencas_assinatura mesmo com a tabela fechada pra "anon"/"authenticated".
export function criarClienteAdmin() {
  return createClient(
    Deno.env.get("SUPABASE_URL")!,
    Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!,
  );
}

export function respostaJson(corpo: unknown, status = 200): Response {
  return new Response(JSON.stringify(corpo), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}
