# KnaulCorp.PersonalTools.Worklog

CLI pessoal para registrar atividades, consultar o tempo de trabalho e exportar relatórios Excel. Interface em português, .NET 10, PostgreSQL, ajuda com destaque âmbar e preenchimento guiado.

## Começar

Requisitos: SDK .NET 10 estável para compilar; runtime .NET 10 para executar a ferramenta instalada; um banco PostgreSQL acessível. A implementação foi testada com PostgreSQL 18.3. Não instala um servidor nem cria seu banco pessoal.

Na pasta do projeto, com PowerShell:

```powershell
./scripts/Install-Worklog.ps1
worklog --help
```

O script compila e instala o pacote local como ferramenta global do seu usuário. Se o terminal ainda não reconhecer o comando, abra um novo terminal ou execute `$HOME/.dotnet/tools/worklog.exe`. O pacote também fica em `artifacts/packages` para reinstalação; nenhum pacote é publicado na internet.

## Configurar seu PostgreSQL

A conexão usa `ConnectionStrings__Worklog`. Seu formato é:

```text
Host=localhost;Port=5432;Database=SEU_BANCO;Username=SEU_USUARIO;Password=SUA_SENHA
```

Forneça a string de conexão localmente, sem colocá-la no histórico do terminal ou no repositório. No PowerShell 7:

```powershell
$env:ConnectionStrings__Worklog = Read-Host 'Cole sua conexão PostgreSQL' -MaskInput
worklog db migrate
worklog doctor
```

A variável acima vale para a sessão atual. Para persistir somente no ambiente do seu usuário, a partir do valor já informado:

```powershell
[Environment]::SetEnvironmentVariable('ConnectionStrings__Worklog', $env:ConnectionStrings__Worklog, 'User')
```

Variáveis de ambiente não são um cofre criptografado. Para desenvolvimento, também é possível configurar a chave `ConnectionStrings:Worklog` com .NET User Secrets. O aplicativo lê User Secrets apenas em Development; a ferramenta instalada usa Production por padrão.

`db migrate` cria/atualiza exclusivamente o schema `worklog`, incluindo o histórico de migrations. O usuário do banco precisa de CONNECT e permissão para criar esse schema e seus objetos. O histórico pessoal fora desse schema não é alterado. Os comandos cotidianos não aplicam migrations automaticamente.

Configurações não sensíveis podem ser sobrescritas em `%LOCALAPPDATA%/KnaulCorp/Worklog/appsettings.json`:

```json
{
  "Worklog": { "TimeZone": "America/Sao_Paulo" }
}
```

Precedência: configuração do aplicativo → configuração por ambiente → User Secrets em Development → arquivo local do usuário → variáveis de ambiente. `worklog config show` nunca imprime a conexão completa. Logs técnicos ficam em `%LOCALAPPDATA%/KnaulCorp/Worklog/logs`, com retenção de 14 arquivos diários e sem descrições pessoais ou senhas.

## Uso diário

```powershell
worklog start
worklog status
worklog stop
worklog list --today
worklog report --today
```

O início pede uma descrição e permite pular os campos opcionais. O encerramento permite registrar o resultado. A atividade continua aberta ao fechar o terminal ou reiniciar o computador: o tempo é calculado pelos horários persistidos.

Com todos os parâmetros, sem perguntas:

```powershell
worklog start --description 'Revisão da integração SAP' --project 'ERP Middleware' --epic SAP --activity 36353 --type 'Revisão de código' --planned yes --origin Analista --non-interactive
worklog stop --result 'Ajustes encaminhados ao desenvolvedor' --non-interactive
```

Há uma atividade aberta por banco pessoal. Para registrar interrupção/dispersão, encerre a atual e inicie outro registro com o tipo correspondente. Pausas/retomadas automáticas ficam para uma evolução futura.

## Lançamentos retroativos e correções

```powershell
worklog add --start 2026-09-28T12:12:00-03:00 --end 2026-09-28T12:35:00-03:00 --project 'ERP Middleware' --epic 'Portal NL' --activity 36347 --type Desenvolvimento --planned yes --origin Analista --description 'Ajustes pós code review com Wander antes de commitar' --result 'ERP Middleware integra com Exact'
worklog stop --at 2026-09-28T17:00:00-03:00 --result 'Concluído'
worklog edit ID_DO_REGISTRO --description 'Descrição corrigida' --clear result
worklog edit ID_DO_REGISTRO --start 2026-09-28T12:10:00-03:00 --end 2026-09-28T12:35:00-03:00
```

Use o ID exibido em `list`, completo ou um prefixo não ambíguo. `Atividade` é o código externo, como 36347, e pode aparecer em diversos registros. O ID interno identifica cada intervalo individualmente.

Datas sem offset são interpretadas no fuso configurado. Horários futuros, fins anteriores/iguais ao início, sobreposições e reabertura de intervalos encerrados são rejeitados. Intervalos adjacentes são permitidos. `edit --end` pode encerrar uma atividade esquecida. Campos omitidos em edição são preservados; `--clear` limpa opcionais e pode ser repetido.

## Relatórios e Excel

```powershell
worklog list --from 2026-09-01 --to 2026-09-30 --project 'ERP Middleware'
worklog report --from 2026-09-01 --to 2026-09-30
worklog export --from 2026-09-01 --to 2026-09-30 --output setembro.xlsx
```

Datas inicial e final são inclusivas. Sem datas, o período é o dia atual. Os filtros por projeto, tipo e atividade são exatos e combinados. A listagem mostra os horários completos do registro original; relatório e Excel recortam o tempo ao período e dividem intervalos que atravessam meia-noite.

Aba **Atividades**, na ordem: Data, Inicio, Fim, Tempo, Projeto, Epic, Atividade, Tipo, Planejado, Interrupção/Dispersão, Origem, Descrição de trabalho, Resultado. A aba **Resumo** apresenta totais por dia, projeto e tipo, fuso e quantidade de registros.

Durações são números formatados como `[h]:mm`, preservando os segundos para cálculos. Totais podem ultrapassar 24 horas. Campos textuais são gravados literalmente, inclusive códigos com zeros à esquerda. Atividades abertas são excluídas dos totais e informadas separadamente. A exportação não precisa do Excel instalado.

Arquivos existentes só são substituídos com `--overwrite`. O diretório de destino deve existir. Períodos vazios geram planilha válida com resumo zerado.

## Automação e terminal

- `--help` em cada comando; `--version` mostra a versão.
- `--non-interactive` impede perguntas. Entrada/saída redirecionada também desativa perguntas.
- `--no-color` remove cores. A ferramenta respeita as capacidades do terminal e a variável `NO_COLOR`.
- O fundo pertence ao terminal; Windows Terminal com tema escuro oferece aparência próxima à referência visual.
- Erros vão para stderr: saída 0 = sucesso, 2 = entrada/regra inválida/cancelamento, 3 = configuração ou banco, 4 = escrita de Excel.

## Desenvolvimento e validação

```powershell
dotnet restore
dotnet build
dotnet test
./scripts/Test-Worklog.ps1
```

`dotnet test` executa testes locais e marca testes PostgreSQL como ignorados quando `WORKLOG_TEST_CONNECTION` não está configurada. O script completo usa Docker para criar um PostgreSQL descartável, executar também a integração e remover somente esse contêiner. Cada teste de integração cria seu próprio banco com prefixo `worklog_test_`; nunca aponte essa variável para o servidor pessoal de produção. Nenhum dado de exemplo é inserido automaticamente no seu banco pessoal.

Para criar futuras migrations: `dotnet tool restore`, configurar a conexão de desenvolvimento e executar `dotnet ef migrations add NOME --project src/KnaulCorp.PersonalTools.Worklog.Infrastructure --startup-project src/KnaulCorp.PersonalTools.Worklog.Infrastructure --output-dir Persistence/Migrations`. A exclusão PostgreSQL `ex_entries_no_overlap` faz parte da migration inicial e deve ser preservada em alterações futuras.

A solution é `KnaulCorp.PersonalTools.Worklog.slnx`. Arquitetura e critérios estão em `specs/approved/2026-09-29-projeto-base-cli`. O diretório físico do checkout ainda conserva o nome anterior TodoList; solution, namespaces, projetos e pacote usam Worklog.

