/*
__________.__        __  .__    .___       __________________      _____    ___________              __    
\______   \__|______/  |_|__| __| _/____   \_   _____/\      \    /  _  \   \__    ___/____    ____ |  | __
 |       _/  \____ \   __\  |/ __ |/ __ \   |    __)  /   |   \  /  /_\  \    |    |  \__  \  /    \|  |/ /
 |    |   \  |  |_> >  | |  / /_/ \  ___/   |     \  /    |    \/    |    \   |    |   / __ \|   |  \    < 
 |____|_  /__|   __/|__| |__\____ |\___  >  \___  /  \____|__  /\____|__  /   |____|  (____  /___|  /__|_ \
        \/   |__|                \/    \/       \/           \/         \/                 \/     \/     \/                                                                                  
                                                              
Copyright Pumpkin Games Ltd. All Rights Reserved.

*/

using System.Runtime.CompilerServices;

namespace Wombat.Engine.Logging;

internal sealed class AnsiParser(Action<string, int, int, ConsoleColor?, ConsoleColor?> onParseWrite)
{
    private readonly Action<string, int, int, ConsoleColor?, ConsoleColor?> _onParseWrite = onParseWrite ?? throw new ArgumentNullException(nameof(onParseWrite));

    internal const string DEFAULT_FOREGROUND_COLOR = "\u001b[39m\u001b[22m";

    internal const string DEFAULT_BACKGROUND_COLOR = "\u001b[49m";

    //
    // Summary:
    //     Parses a subset of display attributes Set Display Attributes Set Attribute Mode
    //     [{attr1};...;{attrn}m Sets multiple display attribute settings. The following
    //     lists standard attributes that are getting parsed: 1 Bright Foreground Colours
    //     30 Black 31 Red 32 Green 33 Yellow 34 Blue 35 Magenta 36 Cyan 37 White Background
    //     Colours 40 Black 41 Red 42 Green 43 Yellow 44 Blue 45 Magenta 46 Cyan 47 White
    public void Parse(string message)
    {
        int num = -1;
        int arg = 0;
        ConsoleColor? arg2 = null;
        ConsoleColor? arg3 = null;
        ReadOnlySpan<char> readOnlySpan = message.AsSpan();
        ConsoleColor? color = null;
        bool isBright = false;
        int num2;
        for (num2 = 0; num2 < readOnlySpan.Length; num2++)
        {
            if (readOnlySpan[num2] == '\u001b' && readOnlySpan.Length >= num2 + 4 && readOnlySpan[num2 + 1] == '[')
            {
                if (readOnlySpan[num2 + 3] == 'm')
                {
                    if (IsDigit(readOnlySpan[num2 + 2]))
                    {
                        int num3 = readOnlySpan[num2 + 2] - 48;
                        if (num != -1)
                        {
                            _onParseWrite(message, num, arg, arg3, arg2);
                            num = -1;
                            arg = 0;
                        }

                        if (num3 == 1)
                        {
                            isBright = true;
                        }

                        num2 += 3;
                        continue;
                    }
                }
                else if (readOnlySpan.Length >= num2 + 5 && readOnlySpan[num2 + 4] == 'm' && IsDigit(readOnlySpan[num2 + 2]) && IsDigit(readOnlySpan[num2 + 3]))
                {
                    int num3 = (readOnlySpan[num2 + 2] - 48) * 10 + (readOnlySpan[num2 + 3] - 48);
                    if (num != -1)
                    {
                        _onParseWrite(message, num, arg, arg3, arg2);
                        num = -1;
                        arg = 0;
                    }

                    if (TryGetForegroundColor(num3, isBright, out color))
                    {
                        arg2 = color;
                        isBright = false;
                    }
                    else if (TryGetBackgroundColor(num3, out color))
                    {
                        arg3 = color;
                    }

                    num2 += 4;
                    continue;
                }
            }

            if (num == -1)
            {
                num = num2;
            }

            int num4 = -1;
            if (num2 < message.Length - 1)
            {
                num4 = message.IndexOf('\u001b', num2 + 1);
            }

            if (num4 < 0)
            {
                arg = message.Length - num;
                break;
            }

            arg = num4 - num;
            num2 = num4 - 1;
        }

        if (num != -1)
        {
            _onParseWrite(message, num, arg, arg3, arg2);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigit(char c)
    {
        return (uint)(c - 48) <= 9u;
    }

    internal static string GetForegroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\u001b[30m",
            ConsoleColor.DarkRed => "\u001b[31m",
            ConsoleColor.DarkGreen => "\u001b[32m",
            ConsoleColor.DarkYellow => "\u001b[33m",
            ConsoleColor.DarkBlue => "\u001b[34m",
            ConsoleColor.DarkMagenta => "\u001b[35m",
            ConsoleColor.DarkCyan => "\u001b[36m",
            ConsoleColor.Gray => "\u001b[37m",
            ConsoleColor.Red => "\u001b[1m\u001b[31m",
            ConsoleColor.Green => "\u001b[1m\u001b[32m",
            ConsoleColor.Yellow => "\u001b[1m\u001b[33m",
            ConsoleColor.Blue => "\u001b[1m\u001b[34m",
            ConsoleColor.Magenta => "\u001b[1m\u001b[35m",
            ConsoleColor.Cyan => "\u001b[1m\u001b[36m",
            ConsoleColor.White => "\u001b[1m\u001b[37m",
            _ => "\u001b[39m\u001b[22m",
        };
    }

    internal static string GetBackgroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\u001b[40m",
            ConsoleColor.DarkRed => "\u001b[41m",
            ConsoleColor.DarkGreen => "\u001b[42m",
            ConsoleColor.DarkYellow => "\u001b[43m",
            ConsoleColor.DarkBlue => "\u001b[44m",
            ConsoleColor.DarkMagenta => "\u001b[45m",
            ConsoleColor.DarkCyan => "\u001b[46m",
            ConsoleColor.Gray => "\u001b[47m",
            _ => "\u001b[49m",
        };
    }

    private static bool TryGetForegroundColor(int number, bool isBright, out ConsoleColor? color)
    {
        color = number switch
        {
            30 => ConsoleColor.Black,
            31 => isBright ? ConsoleColor.Red : ConsoleColor.DarkRed,
            32 => isBright ? ConsoleColor.Green : ConsoleColor.DarkGreen,
            33 => isBright ? ConsoleColor.Yellow : ConsoleColor.DarkYellow,
            34 => (!isBright) ? ConsoleColor.DarkBlue : ConsoleColor.Blue,
            35 => isBright ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta,
            36 => isBright ? ConsoleColor.Cyan : ConsoleColor.DarkCyan,
            37 => isBright ? ConsoleColor.White : ConsoleColor.Gray,
            _ => null,
        };

        if (!color.HasValue)
            return number == 39;

        return true;
    }

    private static bool TryGetBackgroundColor(int number, out ConsoleColor? color)
    {
        color = number switch
        {
            40 => ConsoleColor.Black,
            41 => ConsoleColor.DarkRed,
            42 => ConsoleColor.DarkGreen,
            43 => ConsoleColor.DarkYellow,
            44 => ConsoleColor.DarkBlue,
            45 => ConsoleColor.DarkMagenta,
            46 => ConsoleColor.DarkCyan,
            47 => ConsoleColor.Gray,
            _ => null,
        };

        if (!color.HasValue)
            return number == 49;

        return true;
    }
}