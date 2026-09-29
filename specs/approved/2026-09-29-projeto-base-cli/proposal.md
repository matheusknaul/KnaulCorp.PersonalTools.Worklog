# Proposta — CLI de registro de trabalho

**Estado:** Approved — aprovado explicitamente por Matheus em 29/09/2026.  
**Data:** 29/09/2026.  
**Nome proposto:** KnaulCorp Worklog. **Comando proposto:** `worklog`.

## O que está sendo proposto

Criar uma CLI .NET para registrar intervalos de trabalho e seu contexto no PostgreSQL pessoal, consultar o histórico e exportar relatórios Excel. O primeiro incremento entrega o fluxo completo de instalação local até exportação, com interface inspirada na organização visual do print enviado.

## Por quê

Capturar o horário enquanto a atividade acontece reduz esquecimentos. A correção retroativa e a exportação nas mesmas colunas da planilha tornam o histórico utilizável na rotina atual.

## Skills aplicáveis

- [spec-driven-development](C:/Users/matheus/.codex/skills/spec-driven-development/SKILL.md)
- [coding-rules](C:/Users/matheus/.codex/skills/coding-rules/SKILL.md)
- [clean-architecture](C:/Users/matheus/.codex/skills/clean-architecture/SKILL.md)

## Decisões propostas para este produto

| Tema | Proposta |
|---|---|
| Plataforma | .NET 10 LTS, usando SDK estável; distribuição como .NET Tool em pacote local, sem publicação pública |
| Identidade | Usar KnaulCorp.PersonalTools.Worklog como nome técnico e Worklog na experiência do terminal |
| Terminal | Spectre.Console e Spectre.Console.Cli para apresentação, perguntas e comandos |
| Idioma | Mensagens e ajuda em português; comandos e opções em inglês |
| Dados | PostgreSQL existente, em schema próprio `worklog`, sem alterar tabelas pessoais já existentes |
| Excel | ClosedXML para gerar .xlsx, sem exigir Excel instalado |
| Tempo | Uma atividade aberta por banco pessoal; horário persistido imediatamente, sem cronômetro residente |
| Campos | Descrição curta obrigatória no início; contexto opcional e editável, com sugestões de valores já usados |
| Interrupções | Atividades próprias, classificadas como Interrupção ou Dispersão; encerrar a anterior antes de iniciar outra |
| Configuração | Fuso America/Sao_Paulo inicialmente; conexão por variável de ambiente ou User Secrets no desenvolvimento |

### Ajuste necessário na aplicação da arquitetura

A skill de arquitetura diz que Infrastructure referencia apenas Domain, mas também define contratos em Application implementados por Infrastructure. Para esses contratos (como conexão de consultas e exportação), adotar referência de Infrastructure para Application, preservando a direção das dependências para o núcleo e sem ciclos. A apresentação será uma CLI; os recursos específicos de aplicações web não se aplicam. Esta é uma decisão explícita da proposta para resolver a inconsistência da skill.

## Exemplo de uso proposto

```text
worklog start
worklog status
worklog stop --result "Revisão concluída; ajustes encaminhados"
worklog list --today
worklog export --from 2026-09-01 --to 2026-09-30 --output setembro.xlsx
```

`start` permite o preenchimento guiado. Os mesmos campos poderão ser informados por opções, sem perguntas interativas. O catálogo e os critérios de aceite estão em requirements.md.

## Pendências conhecidas

- Aprovar esta proposta, inclusive nome/comando, escopo e decisões registradas.
- Na configuração real: informar localmente host, porta, banco e usuário e fornecer a senha por configuração privada. Nenhuma credencial precisa ser escrita na spec ou enviada pelo chat.
- Conferir a versão e as permissões do PostgreSQL existente antes de aplicar o schema. Até lá, usar banco de teste isolado.

## Referências técnicas consultadas

- [.NET: política oficial de suporte](https://dotnet.microsoft.com/en-us/platform/support/policy) — .NET 10 é LTS.
- [Spectre.Console](https://spectreconsole.net/) — apresentação no terminal e framework de CLI.
- [ClosedXML](https://docs.closedxml.io/en/latest/) — geração de arquivos Excel sem automação do aplicativo.

Aprovação recebida e spec movida para approved/. O nome técnico KnaulCorp.PersonalTools.Worklog foi solicitado explicitamente pelo usuário.

