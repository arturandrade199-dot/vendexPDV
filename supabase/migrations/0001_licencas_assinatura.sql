create table licencas_assinatura (
    id uuid primary key default gen_random_uuid(),
    email text not null unique,
    nome text,
    fingerprint text,
    status text not null default 'pendente',  -- pendente | ativo | cancelado | atrasado
    hotmart_assinatura_id text,
    data_ultima_cobranca timestamptz,
    data_proxima_cobranca timestamptz,
    criado_em timestamptz not null default now(),
    atualizado_em timestamptz not null default now()
);

create index idx_licencas_assinatura_email on licencas_assinatura (email);

-- Sem acesso público via REST — só as Edge Functions (via service_role, que ignora
-- RLS) tocam essa tabela. Sem policy nenhuma criada aqui de propósito: RLS habilitada
-- e zero policies = nenhum acesso para as roles "anon"/"authenticated".
alter table licencas_assinatura enable row level security;
