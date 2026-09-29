using Spectre.Console;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Application.Reports;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Services;
/// <summary>Apresenta resultados de maneira legível e sem interpretar textos pessoais como marcação.</summary>
public sealed class WorklogUi(IAnsiConsole console, WorklogTime time)
{
    #region Methods
    /// <summary>Mostra a identidade visual na ajuda inicial.</summary>
    public void ShowBanner()
    {
        if (console.Profile.Width >= 76 && !Console.IsOutputRedirected)
        {
            console.Write(new FigletText("WORKLOG").LeftJustified().Color(Color.Orange1));
            console.MarkupLine("[grey]KnaulCorp · v1.0.0[/]");
        }
        else console.MarkupLine("[bold orange1]WORKLOG[/]  [grey]KnaulCorp · v1.0.0[/]");
        console.MarkupLine("[grey]Seu trabalho, registrado no tempo certo.[/]");
        console.WriteLine();
    }
    /// <summary>Mostra confirmação com o identificador completo.</summary>
    public void ShowSaved(string action, Guid id) => console.MarkupLine($"[green]OK[/] {Markup.Escape(action)} [grey]{id}[/]");
    /// <summary>Mostra um aviso literal.</summary>
    public void ShowMessage(string message) => console.WriteLine(message);
    /// <summary>Mostra o registro aberto com tempo decorrido.</summary>
    public void ShowStatus(WorkEntryDto? entry)
    {
        if (entry is null) { console.WriteLine("Nenhuma atividade em andamento. Use worklog start."); return; }
        console.MarkupLine("[orange1 bold]EM ANDAMENTO[/]");
        console.WriteLine(entry.Description);
        console.WriteLine($"Início: {time.ToLocal(entry.StartedAt):dd/MM/yyyy HH:mm:ss}   Tempo: {WorklogTime.FormatDuration(time.Now - entry.StartedAt)}");
        console.WriteLine($"Projeto: {entry.Project ?? "Não informado"}   Atividade: {entry.Activity ?? "Não informada"}");
        console.WriteLine($"ID: {entry.Id}");
        if (DateOnly.FromDateTime(time.ToLocal(entry.StartedAt).DateTime) < time.Today) console.MarkupLine("[yellow]Esta atividade começou em outro dia. Confira o horário antes de encerrar; use stop --at para corrigir.[/]");
        console.MarkupLine("[grey]Use worklog stop para encerrar e registrar o resultado.[/]");
    }
    /// <summary>Mostra registros completos em tabela ou cartões compactos.</summary>
    public void ShowEntries(IReadOnlyList<WorkEntryDto> entries)
    {
        if (entries.Count == 0) { console.WriteLine("Nenhuma atividade no período."); return; }
        if (Console.IsOutputRedirected || console.Profile.Width < 110)
        {
            foreach (WorkEntryDto entry in entries)
            {
                console.WriteLine($"{entry.Id} | {time.ToLocal(entry.StartedAt):dd/MM/yyyy HH:mm:ss} | {(entry.EndedAt is null ? "em andamento" : time.ToLocal(entry.EndedAt.Value).ToString("dd/MM/yyyy HH:mm:ss"))}");
                console.WriteLine($"  {entry.Project ?? "Não informado"} · {entry.Activity ?? "-"} · {entry.Description}");
                console.WriteLine($"  Tempo: {(entry.EndedAt is null ? "em andamento" : WorklogTime.FormatDuration(entry.EndedAt.Value - entry.StartedAt))}");
            }
            return;
        }
        var table = new Table().Border(TableBorder.Rounded).Expand();
        foreach (string header in new[] { "ID", "Início", "Fim", "Tempo", "Projeto", "Descrição" }) table.AddColumn(header);
        foreach (WorkEntryDto entry in entries) table.AddRow(new Text(entry.Id.ToString()), new Text(time.ToLocal(entry.StartedAt).ToString("dd/MM HH:mm")), new Text(entry.EndedAt is null ? "Em andamento" : time.ToLocal(entry.EndedAt.Value).ToString("dd/MM HH:mm")), new Text(entry.EndedAt is null ? "-" : WorklogTime.FormatDuration(entry.EndedAt.Value - entry.StartedAt)), new Text(entry.Project ?? "-"), new Text(entry.Description));
        console.Write(table);
    }
    /// <summary>Mostra totais por dia, projeto e tipo.</summary>
    public void ShowReport(WorkReportDto report)
    {
        console.MarkupLine($"[orange1 bold]Resumo[/] {report.From:dd/MM/yyyy} a {report.To:dd/MM/yyyy}");
        console.WriteLine($"Total: {WorklogTime.FormatDuration(report.Total)} | Encerrados: {report.ClosedCount} | Abertos excluídos: {report.OpenCount}");
        ShowGroups("Por dia", report.Segments.GroupBy(segment => segment.Date.ToString("dd/MM/yyyy")));
        ShowGroups("Por projeto", report.Segments.GroupBy(segment => segment.Entry.Project ?? "Não informado"));
        ShowGroups("Por tipo", report.Segments.GroupBy(segment => segment.Entry.Type ?? "Não informado"));
    }
    private void ShowGroups(string title, IEnumerable<IGrouping<string, ReportSegmentDto>> groups)
    {
        console.MarkupLine($"[orange1]{title}[/]");
        foreach (IGrouping<string, ReportSegmentDto> group in groups) console.WriteLine($"  {group.Key}: {WorklogTime.FormatDuration(TimeSpan.FromTicks(group.Sum(segment => segment.Duration.Ticks)))}");
    }
    #endregion
}
