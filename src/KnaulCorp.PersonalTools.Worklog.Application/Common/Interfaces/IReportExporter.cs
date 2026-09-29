using KnaulCorp.PersonalTools.Worklog.Application.Reports;
namespace KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
/// <summary>Gera um relatório portátil a partir da projeção consolidada.</summary>
public interface IReportExporter
{
    /// <summary>Salva o relatório protegendo arquivos já existentes.</summary>
    Task ExportAsync(WorkReportDto report, string path, bool overwrite, CancellationToken cancellationToken);
}
