# Vendex PDV 1.2.0

## Novo módulo: Controle de Validade

Pensado para padarias, hortifrutis e qualquer loja com produtos perecíveis:

- Cadastro de lotes por produto (com data de fabricação/entrada e validade)
- Cartões de resumo: lotes vencidos, vencendo em 3 e em 7 dias, valor perdido no mês
- Registro de perda por **quantidade exata ou percentual do lote** (útil quando você não
  vai contar unidade por unidade de uma fornada estragada)
- Novo relatório "Perdas por validade no período" (quantidade, valor, % do lote, motivo),
  com exportação em PDF e Excel
- Módulo independente do fluxo de venda — não interfere no estoque nem no PDV, é só
  controle e visibilidade de validade/perda

## Devolução de mercadoria

- Cliente agora é **opcional** na devolução manual — não é mais obrigatório identificar
  o cliente pra registrar uma devolução avulsa
- Busca por lupa para cliente, produto e venda de origem (a venda deixou de ficar
  restrita ao cliente já selecionado — dá pra buscar por número ou por período,
  com o cliente sendo preenchido automaticamente ao escolher a venda)
- Itens da venda de origem são carregados automaticamente ao selecioná-la
- Máscara de telefone no cadastro rápido de cliente
- Acesso à Devolução direto pela tela principal (sidebar e menu), não só de dentro do PDV

## Busca e filtros

- Barra de busca com filtros nas telas de Clientes, Produtos, Fornecedores,
  Contas a Pagar e Contas a Receber

## Ajustes gerais

- Datas em filtros (DatePicker) agora sempre no formato dd/MM/yyyy em todo o sistema
- Sidebar e tela de menu não cortam mais botões em monitores pequenos
- Botão de remover fixo na borda direita das tabelas de itens (Devolução, Controle de
  Validade)

---
_Gerado a partir do histórico de trabalho desde a versão 1.1.0 (commit `6fbd376`)._
