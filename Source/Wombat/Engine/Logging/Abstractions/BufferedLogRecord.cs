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
using System.Diagnostics;

namespace Wombat.Engine.Logging.Abstractions;

/// <summary>
/// Represents a buffered log record to be written in batch to an <see cref="IBufferedLogger" />.
/// </summary>
/// <remarks>
/// Instances of this type may be pooled and reused. Implementations of <see cref="IBufferedLogger" /> must
/// not hold onto instance of <see cref="BufferedLogRecord" /> passed to its <see cref="IBufferedLogger.LogRecords" /> method
/// beyond the invocation of that method.
/// </remarks>
public abstract class BufferedLogRecord
{
    /// <summary>
    /// Gets the time when the log record was first created.
    /// </summary>
    public abstract DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the record's logging severity level.
    /// </summary>
    public abstract LogLevel LogLevel { get; }

    /// <summary>
    /// Gets the record's event id.
    /// </summary>
    public abstract EventId EventId { get; }

    /// <summary>
    /// Gets an exception string for this record.
    /// </summary>
    public virtual string? Exception => null;

    /// <summary>
    /// Gets an activity span ID for this record, representing the state of the thread that created the record.
    /// </summary>
    public virtual ActivitySpanId? ActivitySpanId => null;

    /// <summary>
    /// Gets an activity trace ID for this record, representing the state of the thread that created the record.
    /// </summary>
    public virtual ActivityTraceId? ActivityTraceId => null;

    /// <summary>
    /// Gets the ID of the thread that created the log record.
    /// </summary>
    public virtual int? ManagedThreadId => null;

    /// <summary>
    /// Gets the formatted log message.
    /// </summary>
    public virtual string FormattedMessage => null;

    /// <summary>
    /// Gets the original log message template.
    /// </summary>
    public virtual string MessageTemplate => null;

    /// <summary>
    /// Gets the variable set of name/value pairs associated with the record.
    /// </summary>
    public virtual IReadOnlyList<KeyValuePair<string, object>> Attributes => [];
}
