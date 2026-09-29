# Visão do produto — KnaulCorp Worklog

## Objetivo

Registrar o trabalho diário pelo terminal com pouco atrito, preservando contexto e tempo gasto em um PostgreSQL pessoal, e transformar esses registros em relatórios úteis no Excel.

## Problema

Preencher uma planilha depois do trabalho exige reconstruir horários, interrupções e resultados de memória. O produto deve permitir registrar o início no momento da ação e complementar as informações ao terminar.

## Público-alvo

Uso pessoal de Matheus no trabalho, inicialmente no Windows com PowerShell e PostgreSQL já disponível na própria máquina.

## Experiência desejada

Uma CLI rápida e visualmente cuidada, com identidade KnaulCorp, orientações em português e comandos curtos. Deve funcionar tanto com perguntas guiadas quanto com parâmetros para uso repetitivo. A imagem bunny.net-CLI.png é referência visual, não uma especificação de comandos ou integração com a Bunny.

## Primeira versão

- Iniciar, consultar e encerrar uma atividade; corrigir ou cadastrar registros retroativos.
- Registrar projeto, epic, identificação externa da atividade, classificação, planejamento, interrupção/dispersão, origem, descrição e resultado.
- Consultar períodos e totais; gerar arquivo .xlsx com as colunas fornecidas pelo usuário.
- Persistir no PostgreSQL local sem depender de um terminal aberto ou processo executando continuamente.

## Evolução

Pausas e retomadas com múltiplos segmentos, favoritos, importação de planilhas antigas, integrações com ferramentas de trabalho e relatórios adicionais poderão ser propostos em specs futuras.

## Fora de escopo inicial

Aplicação web, autenticação corporativa, múltiplos usuários, sincronização em nuvem, monitoramento automático do computador, serviço em segundo plano, funcionamento offline e instalação de um novo servidor PostgreSQL.

## Definição de sucesso do produto

O histórico permite reconstruir o dia com confiança e produzir relatórios sem redigitar dados ou calcular tempos manualmente. Iniciar e concluir registros deve se encaixar na rotina sem exigir preencher todos os campos antecipadamente.

