# Vendex — Arquitetura Técnica do Sistema PDV (C# / WPF)

![Ícone Vendex](vendex-icon.png)

## Nome e Identidade

- **Nome escolhido:** Vendex
- **Ícone:** fundo teal escuro com um "X" estilizado formado por setas apontando para cima (remete a venda e crescimento) — `vendex-icon.svg` / `vendex-icon.png`

## Visão Geral

Sistema de ponto de venda (PDV) desktop, standalone, sem necessidade de internet, com **caminho de evolução planejado para operação em rede** (múltiplos terminais compartilhando um banco central) sem reescrever a aplicação. Roda localmente na máquina do cliente. Modelo de licença perpétua sem suporte — vendido pronto para uso imediato.

- **Público-alvo:** mercadinho, quitanda, loja de roupa — comércio informal pequeno
- **Modelo de licença:** perpétua, sem suporte, pagamento único (~R$80)
- **Observação de mercado:** clientes do nicho em geral não querem emitir nota fiscal

Este documento substitui a versão anterior (Lazarus/Free Pascal + Firebird Embedded, ver `arquitetura-pdv_3.md` no projeto Delphi original). Motivo da migração: resultado visual do Lazarus/LCL ficou abaixo do esperado; WPF permite UI muito mais elaborada (temas, animações, data binding) com menos esforço, e o time já vem de Delphi/Object Pascal, então a curva de aprendizado de C# é a mais suave dentre as alternativas .NET.

---

## Stack Tecnológica

| Camada | Tecnologia | Observação |
|---|---|---|
| Linguagem / Runtime | C# / .NET 8 (LTS) | `net8.0-windows` |
| UI | WPF | MVVM |
| Framework MVVM | CommunityToolkit.Mvvm | source generators (`[ObservableProperty]`, `[RelayCommand]`), reduz boilerplate |
| Tema visual | WPF-UI (Fluent Design) | visual nativo Windows 11; alternativa: MaterialDesignInXamlToolkit |
| Banco de Dados | SQLite (arquivo local) | acesso via EF Core |
| ORM | Entity Framework Core 8 + `Microsoft.EntityFrameworkCore.Sqlite` | Code First + Migrations |
| Injeção de Dependência | `Microsoft.Extensions.DependencyInjection` + `Microsoft.Extensions.Hosting` | Generic Host, bootstrap no `App.xaml.cs` |
| Configuração | `appsettings.json` (+ `Microsoft.Extensions.Configuration`) | substitui o `.ini` |
| Hash de senha | `BCrypt.Net-Next` | substitui hash manual do Delphi |
| Relatórios (PDF) | QuestPDF | substitui FastReport, geração de relatório 100% em código |
| Impressão de cupom | Biblioteca ESC/POS (ex.: `ESC-POS-.NET`) | impressoras térmicas |
| Backup | `System.IO.Compression.ZipFile` | rotina automática gerando `.zip` |
| Log/diagnóstico | Serilog (sink de arquivo) | opcional, útil para suporte remoto |
| Testes | xUnit | cobre `Vendex.Domain` e `Vendex.Application` sem precisar de UI nem banco real |
| Instalador | Inno Setup | empacota o publish self-contained do `dotnet publish` |

---

## Arquitetura em Camadas

Ponto central do redesenho: **a camada de dados fica isolada atrás de interfaces**, para que trocar SQLite local por um banco de rede (PostgreSQL/SQL Server via API) no futuro não exija tocar em regra de negócio nem em UI.

```
Vendex.Domain          (entidades + interfaces de repositório — zero dependência externa)
      ↑
Vendex.Application     (casos de uso / services — depende só de Vendex.Domain)
      ↑                                      ↑
Vendex.App (WPF)                    Vendex.Data (EF Core + SQLite)
  Views / ViewModels                  implementa as interfaces de Vendex.Domain
  depende de Vendex.Application
  NUNCA referencia Vendex.Data
  diretamente (só no bootstrap DI)
```

**Regra de dependência:** `Vendex.App` conhece `Vendex.Application` e os *contratos* de `Vendex.Domain`; nunca conhece `Vendex.Data`. A implementação concreta (SQLite hoje, HTTP/API amanhã) é resolvida só no bootstrap de DI (`App.xaml.cs`). ViewModels chamam Services (`Vendex.Application`), Services chamam Repositórios via interface (`IProdutoRepository`, `IVendaRepository`, `IUnitOfWork`...).

Isso também torna `Vendex.Domain` e `Vendex.Application` testáveis com xUnit sem precisar de banco real nem de UI.

---

## Estrutura da Solução

```
VendexPDV.sln
/src
  /Vendex.Domain/            Entidades (POCOs), enums, interfaces de repositório e de Unit of Work
  /Vendex.Application/       Services (casos de uso: VendaService, CaixaService, LicencaService...), DTOs
  /Vendex.Data/               VendexDbContext (EF Core), Migrations, implementações SQLite dos repositórios
  /Vendex.App/                WPF: Views (XAML), ViewModels, App.xaml.cs (bootstrap DI), Assets
  /Vendex.Licensing/          Fingerprint de máquina + validação de serial (compartilhado com GeradorSerial)
/tools
  /GeradorSerial/             App separado (uso exclusivo do vendedor) para gerar seriais de ativação
/docs
  /arquitetura/                Este documento + identidade visual
/installer
  /setup.iss                  Script Inno Setup
```

O `GeradorSerial` continua como projeto/executável separado — nunca entra no instalador do cliente.

---

## Padrão de Acesso a Dados

- **Repository Pattern**: uma interface por agregado (`IProdutoRepository`, `IVendaRepository`, `IClienteRepository`, `ICaixaRepository`, ...) declarada em `Vendex.Domain`, implementada em `Vendex.Data` com EF Core sobre SQLite.
- **Unit of Work** (`IUnitOfWork`): agrupa múltiplos repositórios numa única transação — essencial para operações como fechar uma venda (grava `VENDAS` + `VENDA_ITENS` + baixa de estoque + movimentação de caixa de forma atômica).
- **DbContext único** (`VendexDbContext`) mapeando todas as entidades via Fluent API (`OnModelCreating`), com Migrations do EF Core versionando o schema (substitui o `sql/schema.sql` estático do projeto Delphi).

---

## Modelo de Dados (SQLite via EF Core)

Mesmo modelo conceitual do documento original, com tipos adaptados ao SQLite (`INTEGER PRIMARY KEY AUTOINCREMENT`, `TEXT`, `REAL`/`NUMERIC`, `TEXT` para datas em formato ISO-8601) e `PRAGMA foreign_keys = ON`.

### Tabelas principais

| Tabela | Descrição |
|---|---|
| Produtos | Cadastro de produtos (inclui preço de custo e preço de venda) |
| Vendas | Cabeçalho de cada venda |
| VendaItens | Itens de cada venda |
| Clientes | Cadastro de clientes (vinculado a Contas a Receber) |
| ContasReceber | Lançamentos de valores a receber (venda a prazo, parcelamento) |
| ContasReceberPagamentos | Pagamentos/baixas de contas a receber |
| Fornecedores | Cadastro de fornecedores (vinculado a Contas a Pagar) |
| ContasPagar | Lançamentos de despesas e obrigações a pagar |
| ContasPagarPagamentos | Pagamentos/baixas de contas a pagar |
| Caixa | Abertura e fechamento de caixa (cabeçalho do dia) |
| CaixaAberturaDetalhe | Contagem de cédulas e moedas na abertura do caixa |
| CaixaFechamentoDetalhe | Contagem de cédulas e moedas no fechamento do caixa |
| CaixaMovimentacoes | Sangrias e reforços de caixa durante o dia |
| Usuarios | Login e perfil (Administrador / Funcionário) |
| Modulos | Catálogo dos módulos existentes no sistema |
| UsuarioPermissoes | Vínculo de qual usuário pode acessar qual módulo |
| LogAuditoria | Registro de todas as ações realizadas por usuário |
| Config | Configurações gerais da loja |
| Licenca | Controle de ativação do sistema por serial |

### Clientes

| Campo | Descrição |
|---|---|
| Id | Identificador único |
| Nome | Nome do cliente |
| Telefone | Telefone/WhatsApp para contato |
| Endereco | Endereço (opcional) |
| Documento | CPF (opcional) |
| LimiteCredito | Limite de fiado/crédito permitido para o cliente |
| DataCadastro | Data de cadastro |
| Observacoes | Anotações livres |

### Fornecedores

| Campo | Descrição |
|---|---|
| Id | Identificador único |
| Nome | Nome/razão social do fornecedor |
| Telefone | Telefone/WhatsApp para contato |
| Endereco | Endereço (opcional) |
| Documento | CNPJ/CPF (opcional) |
| DataCadastro | Data de cadastro |
| Observacoes | Anotações livres |

### Contas a Receber

Substitui o antigo módulo de "Fiado", generalizando para qualquer venda a prazo ou parcelada.

**ContasReceber**
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| ClienteId | Referência ao cliente |
| VendaId | Referência à venda de origem (quando aplicável) |
| Descricao | Descrição do lançamento |
| ValorTotal | Valor total a receber |
| DataLancamento | Data em que o débito foi gerado |
| DataVencimento | Data prevista para recebimento |
| Status | Aberto / Parcial / Pago / Atrasado |

**ContasReceberPagamentos**
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| ContaReceberId | Referência ao lançamento em aberto |
| ValorPago | Valor recebido nessa baixa |
| DataPagamento | Data do recebimento |
| FormaPagamento | Dinheiro / Cartão / PIX / Benefícios |

### Contas a Pagar

Controle de despesas e obrigações do comerciante (fornecedores, aluguel, contas fixas, etc.).

**ContasPagar**
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| FornecedorId | Referência ao fornecedor (opcional) |
| Descricao | Descrição da despesa |
| Categoria | Ex: Fornecedor, Aluguel, Energia, Impostos, Outros |
| ValorTotal | Valor total a pagar |
| DataLancamento | Data em que a obrigação foi registrada |
| DataVencimento | Data de vencimento |
| Status | Aberto / Parcial / Pago / Atrasado |

**ContasPagarPagamentos**
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| ContaPagarId | Referência ao lançamento em aberto |
| ValorPago | Valor pago nessa baixa |
| DataPagamento | Data do pagamento |
| FormaPagamento | Dinheiro / Cartão / PIX / Transferência |

---

## Controle de Caixa

Fluxo diário de abertura e fechamento com contagem física de cédulas e moedas, faturamento e lucro apurados automaticamente no fechamento. Toda a operação de fechamento (gravar contagem + calcular divergência + calcular faturamento/lucro) roda dentro de uma única transação via `IUnitOfWork`.

### Abertura do caixa (início do dia)

O operador informa a quantidade de cada cédula e moeda disponível no caixa. O sistema calcula o valor inicial total (**fundo de troco**).

**Caixa** (cabeçalho)
| Campo | Descrição |
|---|---|
| Id | Identificador único do caixa do dia |
| DataAbertura | Data de abertura |
| HoraAbertura | Hora de abertura |
| UsuarioAberturaId | Operador que abriu o caixa |
| ValorAberturaTotal | Soma calculada da contagem de cédulas/moedas |
| DataFechamento | Data de fechamento (nulo enquanto aberto) |
| HoraFechamento | Hora de fechamento |
| UsuarioFechamentoId | Operador que fechou o caixa |
| ValorFechamentoTotal | Soma calculada da contagem final |
| FaturamentoTotal | Total vendido no dia (soma das vendas) |
| CustoTotal | Soma do custo dos produtos vendidos no dia |
| LucroTotal | FaturamentoTotal − CustoTotal |
| Status | Aberto / Fechado |

**CaixaAberturaDetalhe** e **CaixaFechamentoDetalhe** (mesma estrutura, uma para abertura e outra para fechamento)
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| CaixaId | Referência ao caixa do dia |
| TipoCedula | Ex: R$200, R$100, R$50, R$20, R$10, R$5, R$2, R$1, R$0,50, R$0,25, R$0,10, R$0,05 |
| Quantidade | Quantidade contada daquela cédula/moeda |
| Subtotal | TipoCedula × Quantidade |

**CaixaMovimentacoes** (entradas/saídas avulsas durante o dia — sangria, reforço)
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| CaixaId | Referência ao caixa do dia |
| Tipo | Sangria (retirada) / Reforço (entrada) |
| Valor | Valor movimentado |
| Motivo | Descrição/justificativa |
| DataHora | Momento da movimentação |

### Fechamento do caixa (fim do dia)

1. Operador realiza a contagem física final de cédulas e moedas → grava em `CaixaFechamentoDetalhe`.
2. Sistema soma `ValorFechamentoTotal` e compara com `ValorAberturaTotal` + vendas em dinheiro + reforços − sangrias, sinalizando divergência (sobra/falta), se houver.
3. Sistema calcula e exibe no fechamento:
   - **Faturamento do dia** (soma de todas as vendas, independente da forma de pagamento)
   - **Lucro do dia** (faturamento − custo dos produtos vendidos, usando o preço de custo cadastrado em Produtos)

---

## Controle de Ativação (Licenciamento por Serial)

Como o sistema roda 100% offline, a ativação também precisa funcionar sem internet. Mesmo modelo do projeto Delphi: **código de instalação (fingerprint da máquina) + serial gerado por uma ferramenta separada**, de uso exclusivo do vendedor — agora implementado em `Vendex.Licensing`.

### Componentes

- **Vendex.App** — gera o código de instalação e valida o serial informado pelo cliente.
- **GeradorSerial** — ferramenta separada, usada apenas pelo vendedor/admin, que recebe o código de instalação e gera o serial de ativação correspondente.
- **Vendex.Licensing** — biblioteca compartilhada pelos dois com o algoritmo de fingerprint + geração/validação de serial, evitando duplicar essa lógica sensível em dois projetos.

Fingerprint de máquina obtido via `System.Management` (WMI: serial do disco/BIOS/motherboard) combinando os IDs num hash único.

### Fluxo de ativação

1. Cliente instala o `Vendex.App` pela primeira vez.
2. Na primeira execução, o sistema gera um **Código de Instalação** único, calculado a partir do fingerprint da máquina — exibido na tela de ativação.
3. Cliente envia esse Código de Instalação ao vendedor (WhatsApp, e-mail, etc.).
4. Vendedor abre o `GeradorSerial`, informa o Código de Instalação recebido e gera o **Serial de Ativação** correspondente, usando o mesmo algoritmo com chave privada embutida em `Vendex.Licensing`.
5. Cliente digita o Serial de Ativação na tela de ativação do `Vendex.App`.
6. Sistema recalcula o serial esperado a partir do Código de Instalação local + chave embutida e compara com o valor informado. Se bater, libera o uso e grava o status em `Licenca`.

### Licenca

| Campo | Descrição |
|---|---|
| Id | Identificador único |
| CodigoInstalacao | Fingerprint gerado localmente na primeira execução |
| SerialAtivacao | Serial informado pelo cliente na ativação |
| DataAtivacao | Data em que a ativação foi concluída |
| Status | Não Ativado / Ativo / Bloqueado |

> Esse esquema amarra o serial à máquina específica (via fingerprint), evitando que o mesmo serial seja reaproveitado em outro computador.

---

## Controle de Permissões por Usuário

O primeiro usuário criado na ativação do sistema é sempre o **Administrador** (dono da loja). A partir daí, o Administrador pode cadastrar novos usuários (funcionários) e definir, por usuário, quais módulos ele pode acessar.

### Regras

- **Administrador**: acesso irrestrito a todos os módulos, incluindo cadastro de usuários e permissões.
- **Funcionário**: acesso apenas aos módulos liberados pelo Administrador (ex: um caixa/operador pode ter acesso apenas a PDV e Caixa, sem ver Relatórios ou Configurações).

### Estrutura

**Usuarios**
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| Nome | Nome do usuário |
| Login | Usuário de acesso |
| SenhaHash | Senha armazenada com hash (BCrypt) |
| TipoUsuario | Administrador / Funcionário |
| Ativo | Indica se o usuário está ativo ou bloqueado |
| DataCadastro | Data de criação do usuário |

**Modulos** (catálogo fixo dos módulos do sistema)
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| NomeModulo | Ex: PDV, Produtos, Clientes, Fornecedores, Contas a Receber, Contas a Pagar, Caixa, Relatórios, Configurações, Usuários |

**UsuarioPermissoes**
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| UsuarioId | Referência ao usuário |
| ModuloId | Referência ao módulo |
| PodeAcessar | Ver o módulo (Sim / Não) |
| PodeCriar | Criar novos registros no módulo (Sim / Não) |
| PodeEditar | Editar registros existentes no módulo (Sim / Não) |
| PodeExcluir | Excluir/desativar registros no módulo (Sim / Não) |

> Na prática, o Administrador tem uma tela de "Cadastro de Usuários" onde, ao criar ou editar um funcionário, marca em checkboxes — por módulo — o que ele pode ver, criar, editar e excluir. Essa tela em si é restrita apenas ao Administrador. Regra de cascata: desmarcar "Ver" desmarca as outras três (não faz sentido criar/editar/excluir num módulo que não se vê); marcar qualquer uma das outras três marca "Ver" automaticamente.
>
> Nem todo módulo tem as quatro ações mapeadas para um comando real: Clientes não tem ação de excluir/desativar hoje, e em Contas a Receber/Contas a Pagar "Editar" controla a ação de marcar como recebido/pago (não há edição de campos após a criação). Nesses casos o flag correspondente fica salvo no banco mas sem efeito até a tela ganhar a ação equivalente.

Em WPF, a visibilidade dos itens de menu e dos botões de ação por permissão é resolvida no ViewModel (propriedades calculadas a partir das permissões do usuário logado), evitando lógica de autorização espalhada pelas Views. Os comandos (Adicionar/Editar/Excluir) também verificam a permissão internamente antes de executar, como defesa em profundidade — a visibilidade do botão na tela não é a única barreira.

---

## Logger de Auditoria

Registra todas as ações relevantes realizadas dentro do sistema, por usuário, para consulta e auditoria posterior.

**LogAuditoria**
| Campo | Descrição |
|---|---|
| Id | Identificador único |
| UsuarioId | Usuário que realizou a ação |
| DataHora | Data e hora da ação |
| Modulo | Módulo onde a ação ocorreu |
| Acao | Tipo de ação: Inclusão / Alteração / Exclusão / Login / Logout / Cancelamento de venda / Abertura de caixa / Fechamento de caixa / etc. |
| Entidade | Tabela/registro afetado (ex: Vendas, Produtos, ContasPagar) |
| EntidadeId | Id do registro afetado |
| Descricao | Detalhe legível da ação (ex: "Alterou preço do produto X de R$10 para R$12") |

### O que deve ser logado

- Login e logout de usuários
- Inclusão, alteração e exclusão de cadastros (produtos, clientes, fornecedores)
- Vendas realizadas e cancelamentos de venda
- Abertura e fechamento de caixa, incluindo divergências encontradas
- Lançamentos e baixas em Contas a Receber e Contas a Pagar
- Alterações em permissões de usuário
- Tentativas de acesso a módulos sem permissão

Implementação: um `IAuditoriaService` em `Vendex.Application`, injetado nos demais Services, chamado explicitamente nos pontos de negócio relevantes (não via interceptor genérico, para manter a descrição de cada log legível).

Recomendação: a tela de **Relatórios** deve incluir um relatório de auditoria filtrável por usuário, período e módulo, visível apenas para o Administrador.

---

## Relatórios e Impressão

- **Relatórios gerenciais / PDF** (vendas, contas a receber/pagar, auditoria): gerados em código com **QuestPDF**, chamado a partir de `Vendex.Application` (os dados vêm dos Services; o layout do documento fica isolado num projeto ou pasta de "Reports" dedicada).
- **Cupom não-fiscal / recibo de venda**: impressão direta em impressora térmica via comandos ESC/POS, usando uma biblioteca .NET dedicada (ex.: `ESC-POS-.NET`), disparada pelo `VendaService` ao concluir a venda.

---

## Empacotamento e Instalador

### Publicação

```
dotnet publish src/Vendex.App -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Gera um executável self-contained (não exige .NET instalado na máquina do cliente).

### Ferramenta de instalador

**Inno Setup** — gera um instalador `.exe` a partir de um script `.iss`, empacotando o resultado do publish.

### O que entra no instalador

- Executável publicado do `Vendex.App` (self-contained)
- Banco `vendex.db` vazio/modelo (schema já criado via Migrations, sem dados)
- `appsettings.json` padrão
- Ícone da aplicação e atalho de área de trabalho / menu iniciar

> O `GeradorSerial` **não** entra nesse instalador — ele fica só com o vendedor, nunca é distribuído ao cliente.

### O que o instalador faz

1. Copia todos os arquivos para a pasta de instalação escolhida (padrão: `C:\Vendex\`), recriando a estrutura `dados/`, `relatorios/`, `backup/`.
2. Cria atalhos no Menu Iniciar e, opcionalmente, na Área de Trabalho.
3. Registra o desinstalador no Painel de Controle (Adicionar/Remover Programas).
4. Ao final, oferece a opção de abrir o `Vendex.App` imediatamente, iniciando o fluxo de **Ativação** (código de instalação → serial) e cadastro do primeiro usuário (Administrador).

### Desinstalação

- Remove os executáveis e bibliotecas instalados.
- Pergunta se o usuário deseja manter ou apagar a pasta `dados/` (para não perder o histórico de vendas em caso de reinstalação) e os `backup/`.

---

## Caminho de Evolução para Rede (multi-terminal)

Motivo original de exigir a separação forte de camadas: permitir que, no futuro, o Vendex rode em vários terminais compartilhando um único banco, **sem reescrever regra de negócio nem UI**.

Como a migração fica confinada, graças às interfaces em `Vendex.Domain`:

1. **Trocar o provedor do EF Core**: de `Microsoft.EntityFrameworkCore.Sqlite` para `Npgsql.EntityFrameworkCore.PostgreSQL` (ou SQL Server), apontando para um banco central na rede local — a forma mais simples, funciona bem se todos os terminais estiverem na mesma LAN.
2. **Ou introduzir uma API central**: um novo projeto `Vendex.Api` (ASP.NET Core) hospeda `Vendex.Application` + `Vendex.Data` apontando pro banco central; cada terminal passa a rodar um `Vendex.App` "fino", com uma nova implementação dos repositórios (`Vendex.Data.Remote`) que fala HTTP/JSON com a API em vez de acessar o banco diretamente.

Em ambos os casos, a troca acontece **só na camada de dados e no bootstrap de DI** (`App.xaml.cs` registra qual implementação de `IProdutoRepository`, `IVendaRepository` etc. usar). `Vendex.Domain`, `Vendex.Application` e as Views/ViewModels de `Vendex.App` não mudam.

---

## Módulos do MVP

1. **Ativação** — validação de serial na primeira execução (pré-login)
2. **Login**
3. **Menu**
4. **PDV (Venda)**
5. **Produtos**
6. **Clientes** — cadastro vinculado a Contas a Receber
7. **Fornecedores** — cadastro vinculado a Contas a Pagar
8. **Contas a Receber** — controle de vendas a prazo/parceladas (substitui o antigo "Fiado")
9. **Contas a Pagar** — controle de despesas e obrigações do comerciante
10. **Caixa** — abertura com contagem de cédulas/moedas, fechamento com faturamento e lucro do dia
11. **Usuários e Permissões** — cadastro de funcionários e controle de acesso por módulo (restrito ao Administrador)
12. **Relatórios** — inclui relatório de auditoria (log de ações por usuário)
13. **Configurações**

---

## Formas de Pagamento Suportadas

- Dinheiro
- Cartão de Crédito/Débito
- PIX
- Benefícios / Alimentação
- Contas a Receber (venda a prazo, lançada como pendência)
- Fluxos de pagamento mistos (combinação de formas na mesma venda)

---

## Canais de Distribuição Considerados

- Mercado Livre
- Shopee
- Divulgação via TikTok e Instagram

---

*Documento derivado da arquitetura original em Delphi/Lazarus + Firebird Embedded (`arquitetura-pdv_3.md`), redesenhado para C# / WPF / SQLite com camada de dados isolada por interfaces, visando futura evolução para operação em rede.*
