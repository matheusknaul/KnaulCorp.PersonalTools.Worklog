using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Entries.Queries;
using KnaulCorp.PersonalTools.Worklog.Cli.Settings;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Services;
/// <summary>Valida e converte filtros da linha de comando.</summary>
public sealed class PeriodInput(WorklogTime time)
{
    #region Methods
    /// <summary>Obtém datas inclusivas e a consulta de interseção correspondente.</summary>
    public (DateOnly From, DateOnly To, EntrySearchQuery Query) GetPeriod(PeriodSettings settings)
    {
        if (settings.Today && (settings.From is not null || settings.To is not null)) throw new WorklogException("--today não pode ser combinado com --from ou --to.");
        if ((settings.From is null) != (settings.To is null)) throw new WorklogException("Informe --from e --to juntos.");
        DateOnly from = settings.From is null ? time.Today : time.ParseDate(settings.From);
        DateOnly to = settings.To is null ? time.Today : time.ParseDate(settings.To);
        if (to < from) throw new WorklogException("O período está invertido.");
        return (from, to, new(time.GetDayStart(from), time.GetDayStart(to.AddDays(1)), settings.Project?.Trim(), settings.Type?.Trim(), settings.Activity?.Trim()));
    }
    #endregion
}
