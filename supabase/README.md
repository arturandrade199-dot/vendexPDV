# Backend de licenciamento (Supabase)

Cobre a ativação por assinatura (Hotmart → Supabase → Vendex PDV) descrita em
`arquitetura/arquitetura-pdv-csharp.md`. Todo o código aqui é versionado, mas o deploy
em si roda contra a sua própria conta/projeto Supabase — siga os passos abaixo uma vez.

## 1. Criar o projeto e aplicar a migration

```
supabase login
supabase link --project-ref SEU-PROJETO
supabase db push          # aplica supabase/migrations/0001_licencas_assinatura.sql
```

## 2. Gerar as chaves de produção (não reaproveitar as de desenvolvimento)

As chaves que estão no código (`Vendex.Licensing/AssinaturaVerifier.cs`, chave pública)
e comentadas nas Edge Functions são só de desenvolvimento. Antes de vender de verdade:

```
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out private.pem
openssl rsa -pubout -in private.pem -out public.pem
```

- Cole o conteúdo de `private.pem` no secret `RSA_CHAVE_PRIVADA` (passo 3).
- Cole o conteúdo de `public.pem` em `ChavePublicaPem`, em
  `src/Vendex.Licensing/AssinaturaVerifier.cs`, substituindo a chave de desenvolvimento.
- Apague `private.pem` do disco depois de cadastrar o secret — ela nunca deve ser
  commitada nem guardada fora do secret do Supabase.

## 3. Configurar os secrets

```
supabase secrets set RSA_CHAVE_PRIVADA="$(cat private.pem)"
supabase secrets set HOTMART_HOTTOK="o-hottok-da-sua-conta-hotmart"
```

## 4. Publicar as 3 functions

```
supabase functions deploy hotmart-webhook
supabase functions deploy ativar-licenca
supabase functions deploy verificar-licenca
```

`supabase/config.toml` já desliga a exigência de JWT nas 3 — o app conversa direto,
sem login do Supabase Auth envolvido.

## 5. Configurar o webhook na Hotmart

No produto, em "Webhooks": cadastre a URL
`https://SEU-PROJETO.supabase.co/functions/v1/hotmart-webhook` como postback, ativando
pelo menos os eventos de compra aprovada, cobrança recorrente, cancelamento e reembolso.

**Importante**: os nomes de campo usados em `hotmart-webhook/index.ts`
(`data.buyer.email`, `event`, etc.) foram confirmados pela documentação pública da
Hotmart na época em que este código foi escrito, mas o formato já mudou antes. Depois
de configurar o webhook, dispare uma compra de teste na Hotmart e confira em
`supabase functions logs hotmart-webhook` se os campos batem com o payload real —
ajuste o `index.ts` se não bater.

## 6. Apontar o app pra esse projeto

Em `src/Vendex.Application/Services/LicencaService.cs`, troque
`SupabaseFunctionsBaseUrl` pela Project URL real (Painel do Supabase → Configurações do
projeto → API), mantendo o sufixo `/functions/v1`.

## Testando antes de ir pra produção

Sem Docker/Supabase CLI instalado localmente não foi possível rodar `supabase start` e
testar as 3 functions de ponta a ponta nesta sessão — a lógica de assinatura RSA foi
validada separadamente (Node assina, .NET confere, payload adulterado é rejeitado) e a
lógica local de folga offline/relógio atrasado foi testada com um servidor
inacessível de propósito. Falta testar as functions em si: `supabase start` localmente
(exige Docker) ou publicar num projeto de teste e chamar com `curl` antes de apontar
pro `LicencaService` de verdade.
