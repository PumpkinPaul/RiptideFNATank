﻿//-----------------------------------------------------------------------------
// StringBuilderExtensions.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Text;

namespace Wombat.Engine.DebugTools;

/// <summary>
/// Options for StringBuilder extension methods.
/// </summary>
[Flags]
public enum AppendNumberOptions
{
    // Normal format.
    None = 0,

    // Added "+" sign for positive value.
    PositiveSign = 1,

    // Insert Number group separation characters.
    // In Use, added "," for every 3 digits.
    NumberGroup = 2,
}

/// <summary>
/// Static class for string builder extension methods.
/// </summary>
/// <remarks>
/// You can specified StringBuilder for SpriteFont.DrawString from XNA GS 3.0.
/// And you can save unwanted memory allocations.
/// 
/// But there are still problems for adding numerical value to StringBuilder.
/// One of them is boxing occurred when you use StringBuilder.AppendFormat method.
/// Another issue is memory allocation occurred when you specify int or float for
/// StringBuild.Append method.
/// 
/// This class provides solution for those issue.
/// 
/// All methods are defined as extension methods as StringBuilder. So, you can use
/// those method like below.
/// 
/// stringBuilder.AppendNumber(12345);
/// 
/// </remarks>
public static class StringBuilderExtensions
{
    /// <summary>
    /// Cache for NumberGroupSizes of NumberFormat class.
    /// </summary>
    private static readonly int[] numberGroupSizes = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes;

    /// <summary>
    /// string conversion buffer.
    /// </summary>
    private static readonly char[] numberString = new char[32];

    /// <summary>
    /// Convert integer to string and add to string builder.
    /// </summary>
    public static void AppendNumber(this StringBuilder builder, int number)
    {
        AppendNumbernternal(builder, number, 0, AppendNumberOptions.None);
    }

    /// <summary>
    /// Convert integer to string and add to string builder.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="number"></param>
    /// <param name="options">Format options</param>
    public static void AppendNumber(this StringBuilder builder, int number, AppendNumberOptions options)
    {
        AppendNumbernternal(builder, number, 0, options);
    }

    /// <summary>
    /// Convert float to string and add to string builder.
    /// </summary>
    /// <remarks>It shows 2 decimal digits.</remarks>
    public static void AppendNumber(this StringBuilder builder, float number)
    {
        builder.AppendNumber(number, 2, AppendNumberOptions.None);
    }

    /// <summary>
    /// Convert float to string and add to string builder.
    /// </summary>
    /// <remarks>It shows 2 decimal digits.</remarks>
    public static void AppendNumber(this StringBuilder builder, float number, AppendNumberOptions options)
    {
        builder.AppendNumber(number, 2, options);
    }

    /// <summary>
    /// Convert float to string and add to string builder.
    /// </summary>
    public static void AppendNumber(this StringBuilder builder, float number, int decimalCount)
    {
        builder.AppendNumber(number, decimalCount, AppendNumberOptions.None);
    }


    /// <summary>
    /// Convert float to string and add to string builder.
    /// </summary>
    public static void AppendNumber(this StringBuilder builder, float number, int decimalCount, AppendNumberOptions options)
    {
        // Handle NaN, Infinity cases.
        if (float.IsNaN(number))
        {
            builder.Append("NaN");
        }
        else if (float.IsNegativeInfinity(number))
        {
            builder.Append("-Infinity");
        }
        else if (float.IsPositiveInfinity(number))
        {
            builder.Append("+Infinity");
        }
        else
        {
            var intNumber = (int)(number * (float)Math.Pow(10, decimalCount) + 0.5f);

            AppendNumbernternal(builder, intNumber, decimalCount, options);
        }
    }

    private static void AppendNumbernternal(StringBuilder builder, int number, int decimalCount, AppendNumberOptions options)
    {
        // Initialize variables for conversion.
        var nfi = CultureInfo.CurrentCulture.NumberFormat;

        var idx = numberString.Length;
        var decimalPos = idx - decimalCount;

        if (decimalPos == idx)
            decimalPos = idx + 1;

        var numberGroupIdx = 0;
        var numberGroupCount = numberGroupSizes[numberGroupIdx] + decimalCount;

        var showNumberGroup = (options & AppendNumberOptions.NumberGroup) != 0;
        var showPositiveSign = (options & AppendNumberOptions.PositiveSign) != 0;

        var isNegative = number < 0;

        if (number > int.MinValue)
            number = Math.Abs(number);

        // Converting from smallest digit.
        do
        {
            // Add decimal separator ("." in US).
            if (idx == decimalPos)
            {
                numberString[--idx] = nfi.NumberDecimalSeparator[0];
            }

            // Added number group separator ("," in US).
            if (--numberGroupCount < 0 && showNumberGroup)
            {
                numberString[--idx] = nfi.NumberGroupSeparator[0];

                if (numberGroupIdx < numberGroupSizes.Length - 1)
                    numberGroupIdx++;

                numberGroupCount = numberGroupSizes[numberGroupIdx] - 1;
            }

            // Convert current digit to character and add to buffer.
            numberString[--idx] = (char)('0' + number % 10);
            number /= 10;

        } while (number > 0 || decimalPos <= idx);

        // Added sign character if needed.
        if (isNegative)
        {
            numberString[--idx] = nfi.NegativeSign[0];
        }
        else if (showPositiveSign)
        {
            numberString[--idx] = nfi.PositiveSign[0];
        }

        // Added converted string to StringBuilder.
        builder.Append(numberString, idx, numberString.Length - idx);
    }
}