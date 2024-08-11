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
using System.Diagnostics.CodeAnalysis;
using Wombat.Engine.Logging.Abstractions;

namespace Wombat.Engine.Logging;

public sealed class CustomConsoleFormatterOptions : SimpleConsoleFormatterOptions
{
    public string CustomPrefix { get; set; } = "Paul";
    public LogLevel FullColorMessages { get; set; } = LogLevel.Warning;
    public bool WriteCategory { get; set; } = false;
    public bool WriteEventId { get; set; } = false;
}

/// <summary>
/// A simple logger with colours!
/// </summary>
public sealed class CustomConsoleFormatter : ConsoleFormatter, IDisposable
{
    const string LOGLEVEL_PADDING = ": ";
    static readonly string _messagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + LOGLEVEL_PADDING.Length);
    static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;

    static bool IsAndroidOrAppleMobile => OperatingSystem.IsAndroid() ||
                                                  OperatingSystem.IsTvOS() ||
                                                  OperatingSystem.IsIOS(); // returns true on MacCatalyst
    readonly IDisposable _optionsReloadToken;

    public CustomConsoleFormatter(IOptionsMonitor<CustomConsoleFormatterOptions> options)
        : base(nameof(CustomConsoleFormatter))
    {
        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    [MemberNotNull(nameof(FormatterOptions))]
    void ReloadLoggerOptions(CustomConsoleFormatterOptions options)
    {
        FormatterOptions = options;
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    internal CustomConsoleFormatterOptions FormatterOptions { get; set; }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
    {
        if (logEntry.State is BufferedLogRecord bufferedRecord)
        {
            string message = bufferedRecord.FormattedMessage ?? string.Empty;
            WriteInternal(null, textWriter, message, bufferedRecord.LogLevel, bufferedRecord.EventId.Id, bufferedRecord.Exception, logEntry.Category, bufferedRecord.Timestamp);
        }
        else
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return;
            }

            // We extract most of the work into a non-generic method to save code size. If this was left in the generic
            // method, we'd get generic specialization for all TState parameters, but that's unnecessary.
            WriteInternal(scopeProvider, textWriter, message, logEntry.LogLevel, logEntry.EventId.Id, logEntry.Exception?.ToString(), logEntry.Category, GetCurrentDateTime());
        }
    }

    void WriteInternal(IExternalScopeProvider scopeProvider, TextWriter textWriter, string message, LogLevel logLevel,
        int eventId, string exception, string category, DateTimeOffset stamp)
    {
        ConsoleColors logLevelColors = GetLogLevelConsoleColors(logLevel);
        string logLevelString = GetLogLevelString(logLevel);

        string timestamp = null;
        string timestampFormat = FormatterOptions.TimestampFormat;
        if (timestampFormat != null)
        {
            timestamp = stamp.ToString(timestampFormat);
        }
        if (timestamp != null)
        {
            textWriter.Write(timestamp);
        }
        if (logLevelString != null)
        {
            textWriter.WriteColoredMessage(logLevelString, logLevelColors.Background, logLevelColors.Foreground);
        }

        bool singleLine = FormatterOptions.SingleLine;

        // Example:
        // info: ConsoleApp.Program[10]
        //       Request received

        // category and event id
        textWriter.Write(LOGLEVEL_PADDING);

        if (FormatterOptions.WriteCategory)
            textWriter.Write(category);

        if (FormatterOptions.WriteEventId)
        {
            textWriter.Write('[');

            Span<char> span = stackalloc char[10];
            if (eventId.TryFormat(span, out int charsWritten))
                textWriter.Write(span.Slice(0, charsWritten));
            else
                textWriter.Write(eventId.ToString());

            textWriter.Write(']');
        }

        if (!singleLine)
        {
            textWriter.Write(Environment.NewLine);
        }

        // scope information
        WriteScopeInformation(textWriter, scopeProvider, singleLine);
        WriteMessage(textWriter, message, singleLine, logLevel, logLevelColors.Background, logLevelColors.Foreground);

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (exception != null)
        {
            // exception message
            WriteMessage(textWriter, exception, singleLine, logLevel, logLevelColors.Background, logLevelColors.Foreground);
        }
        if (singleLine)
        {
            textWriter.Write(Environment.NewLine);
        }
    }

    void WriteMessage(TextWriter textWriter, string message, bool singleLine, LogLevel logLevel, ConsoleColor? background, ConsoleColor? foreground)
    {
        if (!string.IsNullOrEmpty(message))
        {
            if (singleLine)
            {
                if (FormatterOptions.WriteCategory)
                    textWriter.Write(' ');

                WriteReplacing(textWriter, Environment.NewLine, " ", message, FormatterOptions, logLevel, background, foreground);
            }
            else
            {
                textWriter.Write(_messagePadding);
                WriteReplacing(textWriter, Environment.NewLine, _newLineWithMessagePadding, message, FormatterOptions, logLevel, background, foreground);
                textWriter.Write(Environment.NewLine);
            }
        }

        static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message, CustomConsoleFormatterOptions options, LogLevel logLevel, ConsoleColor? background, ConsoleColor? foreground)
        {
            string newMessage = message.Replace(oldValue, newValue);

            if (logLevel >= options.FullColorMessages)
                writer.WriteColoredMessage(newMessage, background, foreground);
            else
                writer.Write(newMessage);
        }
    }

    DateTimeOffset GetCurrentDateTime()
    {
        return FormatterOptions.TimestampFormat != null
            ? (FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now)
            : DateTimeOffset.MinValue;
    }

    static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    static ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.DarkCyan, ConsoleColor.Black),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkYellow),
            LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
            _ => new ConsoleColors(null, null)
        };
    }

    void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
    {
        if (FormatterOptions.IncludeScopes && scopeProvider != null)
        {
            bool paddingNeeded = !singleLine;
            scopeProvider.ForEachScope((scope, state) =>
            {
                if (paddingNeeded)
                {
                    paddingNeeded = false;
                    state.Write(_messagePadding);
                    state.Write("=> ");
                }
                else
                {
                    state.Write(" => ");
                }
                state.Write(scope);
            }, textWriter);

            if (!paddingNeeded && !singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }

            if (singleLine)
                textWriter.Write(' ');
        }
    }

    readonly record struct ConsoleColors(ConsoleColor? Foreground, ConsoleColor? Background);
}