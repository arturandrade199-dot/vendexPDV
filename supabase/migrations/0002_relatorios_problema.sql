create table relatorios_problema (
    id uuid primary key default gen_random_uuid(),
    fingerprint text not null,
    email text,
    tipo text not null check (tipo in ('automatico', 'manual')),
    mensagem text,
    log text,
    criado_em timestamptz not null default now()
);

-- Usado pela Edge Function reportar-problema pra achar o relatório mais recente de um
-- fingerprint e aplicar o limite de "não manda de novo antes de N minutos".
create index idx_relatorios_problema_fingerprint_criado_em on relatorios_problema (fingerprint, criado_em desc);

-- Mesmo motivo de licencas_assinatura: só as Edge Functions (via service_role, que ignora
-- RLS) tocam essa tabela. RLS habilitada e zero policies = nenhum acesso para "anon"/"authenticated".
alter table relatorios_problema enable row level security;
