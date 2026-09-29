using LiteBus.Commands;
using LiteBus.Queries;
using LiteBus.Messaging;
using LiteBus.Extensions.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Application.Reports;
namespace KnaulCorp.PersonalTools.Worklog.Application;
/// <summary>Registra os casos de uso e os mediadores.</summary>
public static class DependencyInjection
{
    #region Methods
    /// <summary>Adiciona serviços de aplicação.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddTransient(typeof(OperationRunner<>));
        services.AddTransient<ReportBuilder>();
        services.AddLiteBus(builder =>
        {
            builder.AddMessaging(_ => { });
            builder.AddCommands(commands => commands.RegisterFromAssembly(typeof(DependencyInjection).Assembly));
            builder.AddQueries(queries => queries.RegisterFromAssembly(typeof(DependencyInjection).Assembly));
        });
        return services;
    }
    #endregion
}
