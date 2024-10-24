/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Logging;
using Wombat.Engine.Logging.File;

namespace Wombat.Engine.Logging;

/// <summary>
/// A simple console logger with colours!
/// </summary>
public static class Logger
{
    /*
        LogLevel.Trace
        LogLevel.Debug
        LogLevel.Information
        LogLevel.Warning
        LogLevel.Error
        LogLevel.Critical
    */

    static ILoggerFactory _loggerFactory;
    public static ILogger Log;

    static Logger()
    {
        _loggerFactory = LoggerFactory.Create(builder =>
            builder.AddCustomConsoleFormatter(options =>
            {
                options.IncludeScopes = true;
                options.FullColorMessages = LogLevel.Trace;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss.ffffff ";
                options.CustomPrefix = " >>> ";
                options.WriteCategory = true;
            })
            .SetMinimumLevel(LogLevel.Trace)
            .AddFile(options => {
                options.RootPath = AppContext.BaseDirectory;
                options.IncludeScopes = true;
                options.TextBuilder = SingleLineLogEntryTextBuilder.Default;
                options.DateFormat = "HH:mm:ss.ffffff";
                options.Files = [
                    new LogFileOptions { 
                        Path = "test.log",
                    }
                ];
            }));
    }

    public static void CreateLogger(string categoryName) => Log = _loggerFactory.CreateLogger(categoryName);

    // These are used by Riptide for logging.
    public static void Debug(string value) => WriteLineInternal(value, ConsoleColor.DarkCyan);
    public static void Info(string value) => WriteLineInternal(value, ConsoleColor.White);
    public static void Warning(string value) => WriteLineInternal(value, ConsoleColor.Yellow);
    public static void Error(string value) => WriteLineInternal(value, ConsoleColor.Red);

    static void WriteLineInternal(string value, ConsoleColor color)
    {
        //var timestamp = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff}";
        var timestamp = $"{DateTime.UtcNow:HH:mm:ss.ffffff}";
        Console.ForegroundColor = color;
        var message = $"{timestamp} - {value}";
        Console.WriteLine(message);
        System.Diagnostics.Debug.WriteLine(message);

        Console.ResetColor();
    }
}
