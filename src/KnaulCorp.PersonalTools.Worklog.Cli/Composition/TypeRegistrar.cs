using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Composition;
/// <summary>Adapta o registro de comandos à composição de dependências da CLI.</summary>
public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    #region Methods
    /// <inheritdoc />
    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());
    /// <inheritdoc />
    public void Register(Type service, Type implementation) => services.AddTransient(service, implementation);
    /// <inheritdoc />
    public void RegisterInstance(Type service, object implementation) => services.AddSingleton(service, implementation);
    /// <inheritdoc />
    public void RegisterLazy(Type service, Func<object> factory) => services.AddSingleton(service, _ => factory());
    #endregion
}
