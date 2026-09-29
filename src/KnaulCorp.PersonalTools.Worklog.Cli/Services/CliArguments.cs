namespace KnaulCorp.PersonalTools.Worklog.Cli.Services;
/// <summary>Permite opções globais antes do comando e na ajuda principal.</summary>
public static class CliArguments
{
    #region Methods
    /// <summary>Reposiciona somente flags iniciais, preservando valores de opções.</summary>
    public static string[] Normalize(string[] arguments)
    {
        string[] global = ["--no-color", "--non-interactive"];
        if (arguments.All(value => global.Contains(value) || value is "--help" or "-h" or "--version" or "-v")) return arguments.Where(value => !global.Contains(value)).ToArray();
        int leading = arguments.TakeWhile(global.Contains).Count();
        return arguments.Skip(leading).Concat(arguments.Take(leading)).ToArray();
    }
    #endregion
}
