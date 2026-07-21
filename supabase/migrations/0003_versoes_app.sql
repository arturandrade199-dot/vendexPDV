create table versoes_app (
    id uuid primary key default gen_random_uuid(),
    versao text not null,
    url_instalador text not null,
    sha256 text not null,
    notas text,
    criado_em timestamptz not null default now()
);

-- verificar-atualizacao sempre pega a última linha inserida — pressupõe que releases são
-- cadastrados em ordem cronológica crescente de versão (fluxo manual, 1 linha por release).
create index idx_versoes_app_criado_em on versoes_app (criado_em desc);

-- Mesmo motivo das outras tabelas: só a Edge Function (via service_role) toca aqui.
alter table versoes_app enable row level security;
