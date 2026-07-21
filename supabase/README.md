# Backend de licenciamento (Supabase)

Cobre a ativação por assinatura (Hotmart → Supabase → Vendex PDV) descrita em
`arquitetura/arquitetura-pdv-csharp.md`. Todo o código aqui é versionado, mas o deploy
em si roda contra a sua própria conta/projeto Supabase — siga os passos abaixo uma vez.

## 1. Criar o projeto e aplicar a migration

```
supabase login
supabase link --project-ref SEU-PROJETO
supabase db push          # aplica as migrations em supabase/migrations/ (licenças + relatórios de problema)
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

Para o relatório de problemas (ver "Relatório de problemas dos clientes" abaixo),
configure também o envio de email via [Resend](https://resend.com):

```
supabase secrets set RESEND_API_KEY="sua-api-key-do-resend"
supabase secrets set EMAIL_REMETENTE="Vendex PDV <relatorios@seu-dominio.com>"
supabase secrets set EMAIL_DESTINATARIO="seu-email@exemplo.com"
```

`EMAIL_REMETENTE` normalmente precisa ser de um domínio verificado no Resend — o endereço
de teste `onboarding@resend.dev` só entrega pro email usado pra criar a conta no Resend.
Como aqui o destino é sempre a caixa do próprio desenvolvedor, isso não é uma limitação:
basta criar a conta Resend com o mesmo email usado em `EMAIL_DESTINATARIO` e usar
`EMAIL_REMETENTE="onboarding@resend.dev"`, sem precisar cadastrar domínio nenhum. Só vale
a pena verificar um domínio próprio se quiser um remetente com nome customizado ou mandar
pra mais de um email no futuro.

Sem esses 3 secrets, a function `reportar-problema` continua funcionando (grava o
relatório na tabela `relatorios_problema` normalmente), só não manda o email.

## 4. Publicar as 4 functions

```
supabase functions deploy hotmart-webhook
supabase functions deploy ativar-licenca
supabase functions deploy verificar-licenca
supabase functions deploy reportar-problema
```

`supabase/config.toml` já desliga a exigência de JWT nas 4 — o app conversa direto,
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

Em `src/Vendex.Application/SupabaseFunctions.cs`, troque `BaseUrl` pela Project URL real
(Painel do Supabase → Configurações do projeto → API), mantendo o sufixo `/functions/v1`.
Esse valor é usado por todos os serviços (`LicencaService`, `RelatorioProblemaService`).

## Relatório de problemas dos clientes

`reportar-problema` recebe dois tipos de relatório do app:

- **Automático**: `AgendadorRelatorioProblemas` (Vendex.App) roda 1x por dia e só manda
  algo se o log local do dia anterior (`dados/logs/log-AAAA-MM-DD.txt`) tiver alguma
  linha `[ERRO]` ou `[AVISO]` — dias sem problema não geram email.
- **Manual**: tela Configurações → "Reportar um problema", onde o usuário descreve o que
  aconteceu; o log do dia é anexado automaticamente pra dar contexto técnico.

Cada relatório é gravado na tabela `relatorios_problema` (histórico consultável direto
no painel do Supabase, mesmo que o email falhe ou os secrets de email não estejam
configurados) e, se os secrets do Resend estiverem presentes, também dispara um email.
Há um limite de 1 relatório a cada 5 minutos por fingerprint (checado na própria
function) pra não virar spam se algo entrar em loop.

## Testando antes de ir pra produção

Sem Docker/Supabase CLI instalado localmente não foi possível rodar `supabase start` e
testar as functions de ponta a ponta nesta sessão — a lógica de assinatura RSA foi
validada separadamente (Node assina, .NET confere, payload adulterado é rejeitado) e a
lógica local de folga offline/relógio atrasado foi testada com um servidor
inacessível de propósito. Falta testar as functions em si: `supabase start` localmente
(exige Docker) ou publicar num projeto de teste e chamar com `curl` antes de apontar
pro `LicencaService`/`RelatorioProblemaService` de verdade.
