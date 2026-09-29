# Validação da versão 1.0.0

Data: 29/09/2026. Implementação: Codex, a pedido de Matheus.

## Resultado

- Compilação em Release sem avisos ou erros.
- 62 testes aprovados: 18 Domain, 20 Application, 8 Infrastructure e 16 CLI.
- Testes de integração executados contra PostgreSQL 18.3 em contêiner descartável; nenhum teste de integração ignorado na execução completa.
- Pacote local gerado e ferramenta global do usuário instalada como `worklog`.
- Ajuda, versão, diagnóstico, relatório e status executados fora da pasta do repositório.

## Evidências verificadas

- Intervalos e descrições validados, dados normalizados e instantes persistidos em UTC.
- Intervalos adjacentes aceitos; sobreposição concorrente aceita somente uma gravação.
- Edições concorrentes rejeitam versões antigas sem sobrescrever o primeiro resultado.
- Filtros por interseção de período e classificações exatas; horários com offset são convertidos antes da consulta PostgreSQL.
- Exemplos de 23 e 30 minutos totalizam 53 minutos; recortes à meia-noite não duplicam duração; dia de mudança de horário de verão usa tempo efetivamente transcorrido.
- Atividades abertas são excluídas dos totais; totais acima de 24 horas são preservados.
- Excel lido novamente, com verificação também do XML interno: datas e durações numéricas, colunas corretas, código com zeros à esquerda e textos semelhantes a fórmulas preservados literalmente.
- Exportação vazia válida; destino existente protegido contra substituição sem opção explícita.
- Perguntas permitem pular campos, selecionar valor anterior ou digitar valor novo, incluindo colchetes literais.
- Terminal real: ajuda com banner âmbar, versão e seções; opção sem cores; início guiado e encerramento. Renderização estreita de 48 colunas também verificada por teste.
- Ajuda sem banco, falhas de entrada e de configuração verificadas em processos separados com códigos de saída correspondentes.

## Limites da validação

- Microsoft Excel não está instalado/acessível por COM neste ambiente. A abertura visual no aplicativo Excel permanece pendente; leitura do arquivo e estrutura Open XML foram verificadas.
- Conexão e migrations no PostgreSQL pessoal não foram executadas. A configuração privada do usuário é necessária, conforme README.
- O nome físico do checkout continua TodoList. Solution, namespaces, projetos, pacote e comando usam Worklog.
- A spec permanece em approved/ até confirmação da transição de estado prevista no processo solicitado.

## Dependências

Versões diretas fixadas nos projetos; metadados de licença conferidos nos pacotes restaurados. Spectre.Console, LiteBus, ClosedXML e pacotes Microsoft: MIT. Dapper e Serilog: Apache-2.0. Provider Npgsql: PostgreSQL. Testes usam xUnit e FluentAssertions 6.12.2 sob Apache-2.0, além de Spectre.Console.Testing sob MIT.

Para repetir a validação, executar `./scripts/Test-Worklog.ps1`. O script cria o contêiner, executa a suite e encerra somente o ambiente que ele criou.
