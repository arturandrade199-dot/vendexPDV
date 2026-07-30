-- Suporte a compra anual (pagamento único) ao lado da assinatura mensal recorrente.
--
-- A assinatura mensal continua controlada só pela coluna `status`: a Hotmart avisa
-- via webhook (cobrança recorrente aprovada / atrasada / cancelada) e isso já mantém
-- o acesso em dia sozinho. Mas uma compra anual é um pagamento único — a Hotmart
-- manda "aprovado" uma vez e nunca mais avisa nada, então sem uma data de corte o
-- cliente ficaria com acesso vitalício por engano. `expira_em` cobre esse caso.
alter table licencas_assinatura
  add column plano text,               -- 'mensal' | 'anual' — informativo, não usado pra decidir acesso
  add column expira_em timestamptz;    -- data limite de acesso; null = sem prazo fixo (recorrente, via status)

comment on column licencas_assinatura.plano is
  'Detectado no webhook pela presença de data.subscription no payload da Hotmart: presente = mensal (recorrente), ausente = anual (pagamento único).';
comment on column licencas_assinatura.expira_em is
  'Data limite de acesso para compras com prazo fixo (ex.: anual, expira_em = data da compra + 365 dias). Nulo para assinaturas recorrentes, cujo acesso é controlado só pela coluna status.';
