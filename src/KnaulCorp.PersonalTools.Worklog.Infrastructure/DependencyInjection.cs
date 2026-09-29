using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using KnaulCorp.PersonalTools.Worklog.Domain.Interfaces;
using KnaulCorp.PersonalTools.Worklog.Application.Common.Interfaces;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Persistence;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Reports;
using KnaulCorp.PersonalTools.Worklog.Infrastructure.Logging;
namespace KnaulCorp.PersonalTools.Worklog.Infrastructure;
/// <summary>Registra persistência, exportação e observabilidade local.</summary>
public static class DependencyInjection
{
    #region Methods
    /// <summary>Adiciona serviços com inicialização de conexão sob demanda.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connection = configuration.GetConnectionString("Worklog");
        services.AddDbContext<WorklogDbContext>(options => options.UseNpgsql(RequireConnection(connection), provider => provider.MigrationsHistoryTable("__EFMigrationsHistory", "worklog")));
        services.AddSingleton(_ => NpgsqlDataSource.Create(RequireConnection(connection)));
        services.AddScoped<IWorkEntryRepository, WorkEntryRepository>();
        services.AddSingleton<ISqlConnectionFactory, PostgresConnectionFactory>();
        services.AddTransient<IReportExporter, ExcelReportExporter>();
        services.AddSingleton(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));
        services.AddScoped<DatabaseMaintenance>();
        return services;
    }
    private static string RequireConnection(string? value) => string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException("Configure ConnectionStrings__Worklog antes de acessar o banco.") : value;
    #endregion
}
