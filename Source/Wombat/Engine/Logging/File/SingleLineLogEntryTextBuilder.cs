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
using System.Globalization;
using System.Text;

namespace Wombat.Engine.Logging.File;

internal class SingleLineLogEntryTextBuilder : FileLogEntryTextBuilder
{
    public static readonly SingleLineLogEntryTextBuilder Default = new SingleLineLogEntryTextBuilder();

    public override void BuildEntryText(StringBuilder sb, string categoryName, LogLevel logLevel, EventId eventId, string message, Exception exception, IExternalScopeProvider scopeProvider, DateTimeOffset timestamp)
    {
        AppendTimestamp(sb, timestamp);
        AppendSeparator(sb);
        AppendLogLevel(sb, logLevel);
        AppendSeparator(sb);
        AppendCategoryName(sb, categoryName);
        //AppendEventId(sb, eventId);

        if (scopeProvider != null)
        {
            AppendSeparator(sb);
            AppendLogScopeInfo(sb, scopeProvider);
        }

        if (!string.IsNullOrEmpty(message))
        {
            AppendSeparator(sb);
            AppendMessage(sb, message);
        }

        if (exception != null)
        {
            AppendSeparator(sb);
            AppendException(sb, exception);
        }
    }

    void AppendSeparator(StringBuilder sb)
    {
        sb.Append('|');
    }

    protected override void AppendTimestamp(StringBuilder sb, DateTimeOffset timestamp)
    {
        //sb.Append(timestamp.ToLocalTime().ToString("o", CultureInfo.InvariantCulture));
        sb.Append(timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss:ffffff", CultureInfo.InvariantCulture));
    }

    protected override void AppendLogLevel(StringBuilder sb, LogLevel logLevel)
    {
        sb.Append(GetLogLevelString(logLevel));
    }

    protected override void AppendLogScopeInfo(StringBuilder sb, IExternalScopeProvider scopeProvider)
    {
        scopeProvider.ForEachScope((scope, builder) =>
        {
            builder.Append(' ');

            AppendLogScope(builder, scope);
        }, sb);
    }

    protected override void AppendMessage(StringBuilder sb, string message)
    {
        var length = sb.Length;
        sb.AppendLine(message);
        sb.Replace(Environment.NewLine, " ", length, message.Length);
    }
}