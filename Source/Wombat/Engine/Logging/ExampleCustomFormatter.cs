/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Wombat.Engine.Logging;

public sealed class ExampleCustomOptions : SimpleConsoleFormatterOptions
{
    public string CustomPrefix { get; set; } = "Paul";
}

/// <summary>
/// A simple logger with colours!
/// </summary>
public sealed class ExampleCustomFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable _optionsReloadToken;
    private ExampleCustomOptions _formatterOptions;

    public ExampleCustomFormatter(IOptionsMonitor<ExampleCustomOptions> options)
        // Case insensitive
        : base("customName") =>
        (_optionsReloadToken, _formatterOptions) = (options.OnChange(ReloadLoggerOptions), options.CurrentValue);

    private void ReloadLoggerOptions(ExampleCustomOptions options) => _formatterOptions = options;

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider scopeProvider,
        TextWriter textWriter)
    {
        string message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        if (message is null)
            return;

        CustomLogicGoesHere(textWriter);
        textWriter.WriteLine(message);
    }

    private void CustomLogicGoesHere(TextWriter textWriter)
    {
        textWriter.Write(_formatterOptions.CustomPrefix);
    }

    public void Dispose() => _optionsReloadToken?.Dispose();
}
