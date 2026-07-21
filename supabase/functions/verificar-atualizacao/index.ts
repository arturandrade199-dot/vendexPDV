// Devolve a versão mais recente publicada em versoes_app. Leitura pública e não-sensível —
// não precisa de fingerprint nem de assinatura, é só "qual é a última versão disponível".
import { criarClienteAdmin, respostaJson } from "../_shared/supabaseAdmin.ts";

Deno.serve(async (req) => {
  if (req.method !== "GET") return respostaJson({ erro: "método não permitido" }, 405);

  const supabase = criarClienteAdmin();
  const { data, error } = await supabase
    .from("versoes_app")
    .select("versao, url_instalador, sha256, notas")
    .order("criado_em", { ascending: false })
    .limit(1)
    .maybeSingle();

  if (error) {
    console.error(error);
    return respostaJson({ erro: "falha ao consultar versão mais recente" }, 500);
  }

  if (!data) return respostaJson({ erro: "nenhuma versão publicada ainda" }, 404);

  return respostaJson({
    versao: data.versao,
    urlInstalador: data.url_instalador,
    sha256: data.sha256,
    notas: data.notas,
  });
});
