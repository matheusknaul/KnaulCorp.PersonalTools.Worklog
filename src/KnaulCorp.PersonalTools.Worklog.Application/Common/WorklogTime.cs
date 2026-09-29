using System.Globalization;
using System.Text.RegularExpressions;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Application.Common;
/// <summary>Interpreta datas e limites diários no fuso configurado.</summary>
public sealed class WorklogTime(TimeZoneInfo zone, TimeProvider clock)
{
    #region Properties
    /// <summary>Fuso aplicado às entradas e aos relatórios.</summary>
    public TimeZoneInfo Zone => zone;
    /// <summary>Instante atual.</summary>
    public DateTimeOffset Now => clock.GetUtcNow();
    /// <summary>Data local atual.</summary>
    public DateOnly Today => DateOnly.FromDateTime(ToLocal(Now).DateTime);
    #endregion
    #region Methods
    /// <summary>Converte um instante para o fuso de exibição.</summary>
    public DateTimeOffset ToLocal(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, zone);
    /// <summary>Interpreta uma data ISO.</summary>
    public DateOnly ParseDate(string value) => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date) && date.Year < 9999 ? date : throw new WorklogException("Use uma data válida no formato YYYY-MM-DD (ano inferior a 9999).");
    /// <summary>Interpreta data e hora ISO, exigindo offset em horários ambíguos.</summary>
    public DateTimeOffset ParseInstant(string value)
    {
        if (!Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2})?(Z|[+-]\d{2}:\d{2})?$")) throw new WorklogException("Use ISO 8601, por exemplo 2026-09-28T12:12:00-03:00.");
        if (Regex.IsMatch(value, @"(Z|[+-]\d{2}:\d{2})$") && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset instant)) return instant.ToUniversalTime();
        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime local)) throw new WorklogException("Data e hora inválidas.");
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local) || zone.IsAmbiguousTime(local)) throw new WorklogException("Horário local inválido ou ambíguo. Informe o offset explicitamente.");
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, zone));
    }
    /// <summary>Obtém o primeiro instante válido do dia local.</summary>
    public DateTimeOffset GetDayStart(DateOnly date)
    {
        DateTime local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        while (zone.IsInvalidTime(local)) local = local.AddMinutes(1);
        TimeSpan offset = zone.IsAmbiguousTime(local) ? zone.GetAmbiguousTimeOffsets(local).Max() : zone.GetUtcOffset(local);
        return new DateTimeOffset(local, offset).ToUniversalTime();
    }
    /// <summary>Formata duração sem limitar o número de horas a 24.</summary>
    public static string FormatDuration(TimeSpan duration) => $"{(long)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}";
    #endregion
}
