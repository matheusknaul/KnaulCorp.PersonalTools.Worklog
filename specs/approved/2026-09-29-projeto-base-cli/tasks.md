# Tarefas — CLI de registro de trabalho

**Estado:** Approved. Implementação autorizada por Matheus em 29/09/2026.

## Decisões registradas para aprovação

- [x] Aprovar nome Worklog, comando worklog e escopo de uso pessoal no Windows.
- [x] Aprovar uma atividade aberta por vez, bloqueio de sobreposição e interrupções como registros separados.
- [x] Aprovar campos opcionais, descrição obrigatória, fuso inicial e tratamento de virada de dia.
- [x] Aprovar catálogo de comandos, relatório e ajuste de dependência descritos na proposta.

## Implementação após aprovação

- [x] Preparar solução e empacotamento local da CLI, fixando SDK estável e dependências compatíveis.
- [x] Modelar intervalo de trabalho, classificações, início/fim e correções, protegendo as regras temporais e de concorrência.
- [x] Implementar persistência no schema worklog, controle de edição concorrente e migrations explícitas sem afetar tabelas existentes.
- [x] Entregar start, stop, status, add e edit com identificação inequívoca dos registros e sugestões de valores usados.
- [x] Entregar list e report com filtros, divisão por dia e agregações consistentes.
- [x] Entregar export com abas Atividades e Resumo, tipos numéricos de data/tempo, textos seguros e proteção contra sobrescrita involuntária.
- [x] Construir identidade visual Worklog, ajuda, perguntas guiadas, saída compacta e comportamento sem interação.
- [x] Entregar config show e doctor, mais instruções de configuração privada, preparação do schema e instalação local.
- [x] Validar cálculos, falhas esperadas e concorrência contra PostgreSQL de teste isolado; executar os cenários de aceite de requirements.md.
- [x] Verificar o arquivo Excel gerado e a apresentação real no terminal, registrando os resultados e limitações da validação.
- [x] Documentar uso diário com os exemplos do usuário; manter esses exemplos fora do banco pessoal até solicitação explícita de cadastro.

## Pendências antes/durante a implementação

- [x] Receber aprovação explícita e mover esta pasta de draft/ para approved/.
- [x] Validar versões e licenças das dependências escolhidas antes de fixá-las.
- [ ] Configurar acesso ao PostgreSQL real por meio privado e confirmar permissões para o schema próprio; não é requisito para escrever código ou testar em banco isolado.
- [x] Se o ambiente não permitir testes de integração ou abertura no Excel, registrar precisamente as verificações pendentes.
- [ ] Ao concluir, apresentar os resultados e solicitar a transição para implemented/ com assinatura correta de autoria e data real; arquivamento depende de confirmação posterior.

Resultados da validação registrados em [docs/validation.md](../../../docs/validation.md).

