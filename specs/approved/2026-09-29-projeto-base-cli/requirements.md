# Requisitos — CLI de registro de trabalho

**Estado:** Approved. Critérios aprovados por Matheus em 29/09/2026; implementação em andamento.

## 1. Instalação e configuração

- [x] Compilar com .NET 10 estável e instalar localmente como .NET Tool com comando `worklog` disponível fora do repositório.
- [x] Seguir as skills declaradas na proposta, incluindo o ajuste de dependência ali documentado.
- [x] `worklog config show` exibe fuso e configuração efetiva não sensível, sem senha nem string de conexão completa.
- [x] Ler a conexão pela chave ConnectionStrings:Worklog (variável `ConnectionStrings__Worklog`); configurações não sensíveis têm defaults no appsettings.json e podem ser sobrescritas externamente.
- [x] `worklog doctor` verifica configuração, conectividade e versão do schema, com erro legível e ação sugerida.
- [x] `worklog db migrate` aplica migrations explicitamente ao schema `worklog`; nenhum comando de atividade aplica migrations automaticamente ou cria um servidor/banco PostgreSQL.
- [x] Não versionar segredos, incluir descrições pessoais em logs técnicos ou manipular tabelas fora do schema da aplicação.

## 2. Registro e campos

Cada registro representa um intervalo contínuo. Possui identificador interno único, início, fim opcional e os campos abaixo. O código externo em Atividade não é a identidade do registro: várias sessões podem apontar para a mesma atividade.

| Coluna do relatório, nesta ordem | Regra |
|---|---|
| Data | Derivada do início no fuso configurado; não preenchida separadamente |
| Inicio | Horário inicial; instante completo preservado no banco |
| Fim | Horário final; ausente enquanto em andamento |
| Tempo | Calculado a partir dos instantes; não editado diretamente |
| Projeto | Texto opcional, por exemplo ERP Middleware |
| Epic | Texto opcional, por exemplo Portal NL ou SAP |
| Atividade | Código externo opcional armazenado como texto, preservando zeros e letras |
| Tipo | Texto opcional; sugestões iniciais: Desenvolvimento, Revisão de código, Reunião, Suporte, Estudo, Interrupção e Dispersão |
| Planejado | Sim, Não ou não informado; não presumir Sim |
| Interrupção/Dispersão | Motivo/contexto opcional; vazio apresentado como N/A |
| Origem | Texto opcional; sugestões iniciais: Analista, Própria, Equipe e Gestor |
| Descrição de trabalho | Texto obrigatório e não vazio; editável posteriormente |
| Resultado | Texto opcional, solicitado ao encerrar no modo guiado, com opção de pular |

- [x] Aceitar acentos, espaços e quebras de linha nos textos pertinentes. Valores de catálogo são sugestões, permitindo texto livre.
- [x] Sugerir projetos, epics, tipos e origens já registrados, sem exigir cadastros prévios.
- [x] Trim nas bordas; descrição de até 4.000 caracteres, resultado e motivo de até 4.000, campos de classificação de até 200 e código externo de até 100.
- [x] Campos opcionais não informados permanecem nulos, sem inventar dados. A exibição de N/A não substitui o valor armazenado.

## 3. Ciclo de vida e consistência

- [x] `start` registra o instante atual como início por padrão. A descrição é o único texto obrigatório; depois de persistir, o comando termina e libera o terminal.
- [x] `status` mostra a atividade aberta, início e tempo decorrido; sem atividade, apresenta mensagem clara. Não depende de memória do processo anterior.
- [x] `stop` encerra a atividade aberta no instante atual por padrão e permite atualizar descrição, resultado e classificações.
- [x] Rejeitar segundo `start` enquanto houver atividade aberta, inclusive com dois terminais concorrentes. Mostrar a atividade existente e orientar o encerramento.
- [x] `stop` sem atividade aberta retorna erro sem criar registro; duas tentativas simultâneas não sobrescrevem um encerramento já confirmado.
- [x] Fechar terminal ou reiniciar a máquina não encerra a atividade. Um intervalo aberto antigo é destacado no próximo status para correção explícita.
- [x] `add` cadastra intervalo já encerrado com início e fim explícitos; `edit <id>` corrige dados e horários, podendo encerrar um registro aberto mediante fim explícito.
- [x] `edit` não reabre um registro encerrado; exige ao menos uma mudança e permite limpar campos opcionais por `--clear <nome-da-opcao>` repetível. Uma opção não pode ser definida e limpa na mesma chamada.
- [x] Duração é fim menos início, com precisão de segundos. Rejeitar fim igual ou anterior ao início e instantes futuros.
- [x] Rejeitar sobreposição entre intervalos, inclusive lançamentos retroativos, edições e concorrência. O intervalo é fechado no início e aberto no fim: um registro pode terminar exatamente quando outro começa.
- [x] Intervalo aberto ocupa desde seu início em diante. Lacunas entre registros são permitidas e não contam como trabalho.
- [x] Datas explícitas usam ISO 8601: `2026-09-28T12:12:00-03:00`; sem offset, interpretar no fuso configurado, rejeitando horários locais inválidos ou ambíguos com orientação para informar offset.
- [x] Persistir instantes em UTC e converter para o fuso configurado na exibição. Viradas de dia não produzem duração negativa.
- [x] Falha de banco não produz mensagem de sucesso nem armazenamento offline implícito. Alterações são atômicas; o histórico previamente confirmado permanece consultável após reconexão.

## 4. Comandos e uso sem interação

| Comando | Função |
|---|---|
| `worklog` / `worklog --help` | Identidade, comandos, opções e exemplos, mesmo sem banco configurado |
| `worklog start` | Iniciar registro, com perguntas quando faltarem os dados no modo interativo |
| `worklog status` | Consultar registro aberto |
| `worklog stop` | Encerrar registro aberto e complementar informações |
| `worklog add` | Lançar intervalo encerrado retroativamente |
| `worklog edit <id>` | Corrigir um registro pelo identificador interno |
| `worklog list --today` | Listar registros do dia |
| `worklog list --from <data> --to <data>` | Consultar período; filtros opcionais por projeto, tipo e atividade |
| `worklog report --from <data> --to <data>` | Mostrar totais por dia, projeto e tipo |
| `worklog export --from <data> --to <data> --output <arquivo.xlsx>` | Gerar planilha de atividades e resumo |
| `worklog config show` | Consultar configuração não sensível |
| `worklog doctor` | Diagnosticar acesso ao banco e schema |
| `worklog db migrate` | Preparar/atualizar schema explicitamente |

- [x] Opções de registro: `--description`, `--project`, `--epic`, `--activity`, `--type`, `--planned yes|no`, `--interruption`, `--origin`, `--result`; cada comando só aceita campos pertinentes.
- [x] `--at` define início no start ou fim no stop; add e edit usam `--start` e `--end`. No add ambos são obrigatórios; no edit ausências preservam os valores existentes.
- [x] Oferecer `--help` por comando, `--version`, `--no-color` e `--non-interactive`.
- [x] Com entrada redirecionada ou `--non-interactive`, nunca aguardar perguntas: exigir entradas obrigatórias, aceitar opcionais ausentes e retornar erros claros.
- [x] Retornar código 0 em sucesso, 2 para entrada/regra inválida, 3 para falha de configuração/conexão/persistência e 4 para falha de escrita da exportação; erros seguem para stderr.
- [x] Cancelar perguntas antes da confirmação de persistência não grava um registro parcial.
- [x] Identificador interno completo disponível na listagem; se houver abreviação visual, aceitar apenas prefixos não ambíguos.

Exemplos do contrato proposto:

```text
worklog start --description "Revisão da integração SAP" --project "ERP Middleware" --epic SAP --activity 36353 --type "Revisão de código" --planned yes --origin Analista
worklog stop --result "Ajustes encaminhados ao desenvolvedor"
worklog add --start 2026-09-28T12:12:00-03:00 --end 2026-09-28T12:35:00-03:00 --project "ERP Middleware" --epic "Portal NL" --activity 36347 --type Desenvolvimento --planned yes --origin Analista --description "Ajustes pós code review com Wander antes de commitar" --result "ERP Middleware integra com Exact"
worklog export --from 2026-09-01 --to 2026-09-30 --output setembro.xlsx
```

## 5. Apresentação no terminal

- [x] Banner tipográfico próprio Worklog, versão discreta, cor de destaque laranja/âmbar, textos secundários suaves e seções legíveis de comandos/opções/exemplos.
- [x] Preservar o fundo do terminal: o fundo escuro do print é uma configuração do terminal, não uma janela gráfica a recriar.
- [x] Exibir banner na ajuda inicial; comandos cotidianos usam respostas compactas com estado, horário e próxima ação útil.
- [x] Perguntas guiadas com seleção e texto livre; não exigir completar todos os campos opcionais.
- [x] Adaptar tabelas à largura e disponibilizar texto simples quando não houver ANSI/Unicode ou quando a saída for redirecionada; `--no-color` remove sequências de cor.
- [x] Renderizar textos fornecidos pelo usuário literalmente, incluindo colchetes, sem interpretá-los como marcação do terminal.

Prévia textual de hierarquia visual (as cores serão aplicadas na implementação):

```text
WORKLOG                                      KnaulCorp
Seu trabalho, registrado no tempo certo.

Comandos
  start       Começar uma atividade
  stop        Encerrar e registrar o resultado
  status      Ver o que está em andamento
  list        Consultar o histórico
  report      Consultar totais do período
  export      Gerar relatório Excel

Exemplos
  $ worklog start
  $ worklog list --today

Use worklog <comando> --help para ver todas as opções.
```

## 6. Consultas e Excel

- [x] `--from` e `--to` recebem YYYY-MM-DD; ambas as datas são inclusivas no fuso configurado. Rejeitar intervalo invertido. `--today` não pode ser combinado com datas explícitas.
- [x] Listagem sem período usa o dia atual, ordena por início e inclui atividades abertas que intersectem o período; filtros exatos de projeto/tipo e código de atividade combinam por AND.
- [x] Intervalos atravessando meia-noite são divididos por dia somente na projeção do relatório, preservando um único registro original. Filtros contam apenas a parte dentro do período solicitado.
- [x] Relatórios e exportação somam somente atividades encerradas. Atividades abertas pertinentes são informadas como excluídas dos totais, sem encerramento automático.
- [x] Aba `Atividades` contém exatamente as 13 colunas da seção 2, na mesma ordem. Datas e horários são células numéricas, e Tempo é duração numérica com formato `[h]:mm`, somável acima de 24 horas; segundos são preservados no valor.
- [x] Usar dd/mm/yyyy para Data, hh:mm:ss para Inicio/Fim, cabeçalho destacado, filtros, congelamento da primeira linha e quebra de texto nas descrições e resultados.
- [x] Aba `Resumo` informa período, fuso, quantidade de registros originais encerrados, quantidade de abertos excluídos e totais por dia/projeto/tipo. Campos de agrupamento vazios aparecem como Não informado.
- [x] Totais somam valores numéricos completos, sem somar durações previamente arredondadas para apresentação. Partes divididas na meia-noite não duplicam o total.
- [x] Trecho diário que termina à meia-noite exibe Fim como 00:00:00; nota na aba Resumo explica que corresponde ao início do dia seguinte e que Tempo utiliza os instantes completos.
- [x] Campo textual que começa com `=`, `+`, `-` ou `@` é gravado como texto, nunca como fórmula; Atividade permanece texto mesmo quando contém só números.
- [x] Não sobrescrever arquivo existente sem `--overwrite`; escrita malsucedida não substitui arquivo anterior por um arquivo incompleto.
- [x] Período vazio gera arquivo válido com cabeçalhos e resumo zerado, com mensagem explícita.

## 7. Cenários de aceite verificáveis

- [x] Em banco de teste, 28/09/2026 das 12:12 às 12:35 resulta em 00:23; das 13:50 às 14:20 resulta em 00:30; resumo do dia resulta em 00:53.
- [x] Em 18/09/2026, 15:15–16:34 resulta em 01:19 e a Dispersão 16:34–17:00 resulta em 00:26, somando 01:45. Campos ausentes podem permanecer vazios; uma descrição deve ser fornecida para cadastrar esses exemplos.
- [x] Intervalo 23:50–00:10 aparece como duas partes de 00:10 nos respectivos dias; a consulta abrangendo ambos soma 00:20.
- [x] Dois processos concorrentes não criam duas atividades abertas nem intervalos sobrepostos; uma edição concorrente não perde alterações silenciosamente.
- [x] Abrir novo terminal após start mantém o status; encerrar com horário retroativo corrige uma atividade esquecida sem alterar outras.
- [ ] Arquivo exportado pode ser lido de volta e aberto no Excel, com colunas, tipos de células, durações, acentos e totais corretos, incluindo total acima de 24 horas.
- [x] Verificar visualmente ajuda, perguntas, status, listagem e erros em terminal comum e estreito, além de saída sem cores.

## Registro de validação

Implementação concluída e testes aprovados; evidências em [docs/validation.md](../../../docs/validation.md). A leitura do arquivo Excel foi verificada, mas a abertura visual no Microsoft Excel permanece pendente porque o aplicativo não está disponível. O acesso ao PostgreSQL pessoal depende de configuração privada do usuário; os critérios de banco foram verificados em PostgreSQL de teste isolado.

