using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
namespace KnaulCorp.PersonalTools.Worklog.Cli.Composition;
/// <summary>Adaptador de composição exigido pelo Spectre.Console.Cli.</summary>
public sealed class TypeResolver(ServiceProvider provider) : ITypeResolver, IDisposable
{
    #region Methods
    /// <inheritdoc />
    public object? Resolve(Type? type) => type is null ? null : provider.GetService(type);
    /// <inheritdoc />
    public void Dispose() => provider.Dispose();
    #endregion
}
