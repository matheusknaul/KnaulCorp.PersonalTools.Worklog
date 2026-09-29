using System.Globalization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using KnaulCorp.PersonalTools.Worklog.Application;
using KnaulCorp.PersonalTools.Worklog.Application.Common;
using KnaulCorp.PersonalTools.Worklog.Cli.Commands;
using KnaulCorp.PersonalTools.Worklog.Cli.Composition;
using KnaulCorp.PersonalTools.Worklog.Cli.Services;
using KnaulCorp.PersonalTools.Worklog.Infrastructure;
using KnaulCorp.PersonalTools.Worklog.Domain.Exceptions;
namespace KnaulCorp.PersonalTools.Worklog.Cli;
/// <summary>Ponto de composição e execução do Worklog.</summary>
public static class Program
{
    #region Methods
    /// <summary>Executa a CLI e traduz falhas em códigos de saída previsíveis.</summary>
    public static async Task<int> Main(string[] args)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; cancellation.Cancel(); };
        try
        {
            string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environment}.json", true);
            if (environment == "Development") configurationBuilder.AddUserSecrets(typeof(Program).Assembly, true);
            string userConfiguration = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KnaulCorp", "Worklog", "appsettings.json");
            configurationBuilder.AddJsonFile(userConfiguration, true).AddEnvironmentVariables();
            IConfigurationRoot configuration = configurationBuilder.Build();
            using Serilog.Core.Logger logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            bool noColor = args.Contains("--no-color") || Console.IsOutputRedirected;
            var consoleSettings = new AnsiConsoleSettings
            {
                Ansi = noColor ? AnsiSupport.No : AnsiSupport.Detect,
                ColorSystem = noColor ? ColorSystemSupport.NoColors : ColorSystemSupport.Detect
            };
            IAnsiConsole console = AnsiConsole.Create(consoleSettings);
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Worklog:TimeZone"] ?? throw new InvalidOperationException("Fuso não configurado."));
            var time = new WorklogTime(zone, TimeProvider.System);
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<Serilog.ILogger>(logger);
            services.AddSingleton(console);
            services.AddSingleton(time);
            services.AddApplication().AddInfrastructure(configuration);
            services.AddTransient<EntryInput>();
            services.AddTransient<OptionalInput>();
            services.AddTransient<PeriodInput>();
            services.AddTransient<WorklogUi>();
            var application = new CommandApp(new TypeRegistrar(services));
            application.Configure(config =>
            {
                config.SetApplicationName("worklog");
                config.Settings.Console = console;
                config.SetApplicationVersion("1.0.0");
                config.SetApplicationCulture(CultureInfo.GetCultureInfo("pt-BR"));
                config.PropagateExceptions();
                config.UseStrictParsing();
                var accent = new Style(Color.Orange1, decoration: Decoration.Bold);
                HelpProviderStyle helpStyles = HelpProviderStyle.Default;
                helpStyles.Description!.Header = accent;
                helpStyles.Usage!.Header = accent;
                helpStyles.Examples!.Header = accent;
                helpStyles.Options!.Header = accent;
                helpStyles.Arguments!.Header = accent;
                helpStyles.Commands!.Header = accent;
                config.Settings.HelpProviderStyles = helpStyles;
                config.AddCommand<StartCommand>("start").WithDescription("Começar uma atividade.").WithExample("start");
                config.AddCommand<StopCommand>("stop").WithDescription("Encerrar a atividade e registrar o resultado.");
                config.AddCommand<StatusCommand>("status").WithDescription("Ver a atividade em andamento.");
                config.AddCommand<AddCommand>("add").WithDescription("Registrar intervalo retroativo.");
                config.AddCommand<EditCommand>("edit").WithDescription("Corrigir um registro existente.");
                config.AddCommand<ListCommand>("list").WithDescription("Consultar o histórico.").WithExample("list", "--today");
                config.AddCommand<ReportCommand>("report").WithDescription("Consultar totais por dia, projeto e tipo.");
                config.AddCommand<ExportCommand>("export").WithDescription("Gerar relatório Excel.");
                config.AddCommand<DoctorCommand>("doctor").WithDescription("Verificar conexão e schema.");
                config.AddBranch("config", branch =>
                {
                    branch.SetDescription("Consultar a configuração local.");
                    branch.AddCommand<ConfigShowCommand>("show").WithDescription("Exibir configuração sem segredos.");
                });
                config.AddBranch("db", branch =>
                {
                    branch.SetDescription("Preparar o schema de atividades.");
                    branch.AddCommand<MigrateCommand>("migrate").WithDescription("Aplicar migrations explicitamente.");
                });
            });
            args = CliArguments.Normalize(args);
            if (args.Length == 0 || args.All(value => value is "--help" or "-h")) new WorklogUi(console, time).ShowBanner();
            try { return await application.RunAsync(args, cancellation.Token); }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                logger.Error("Falha CLI {ErrorType}", exception.GetType().Name);
                throw;
            }
        }
        catch (Exception exception) when (exception is WorklogException or ValidationException or CommandParseException)
        {
            await Console.Error.WriteLineAsync(exception is CommandParseException ? "Comando ou opções inválidas. Use worklog --help ou worklog <comando> --help." : exception.Message);
            return 2;
        }
        catch (OperationCanceledException)
        {
            await Console.Error.WriteLineAsync("Operação cancelada.");
            return 2;
        }
        catch (Exception exception) when (exception is NpgsqlException or DbUpdateException or InvalidOperationException or ArgumentException or IOException or UnauthorizedAccessException or TimeZoneNotFoundException or InvalidTimeZoneException or FormatException or CommandRuntimeException)
        {
            await Console.Error.WriteLineAsync("Falha de configuração ou acesso ao banco. Confira ConnectionStrings__Worklog e o fuso; execute worklog doctor e, se necessário, worklog db migrate.");
            return 3;
        }
    }
    #endregion
}


