// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

namespace AzureApp
{
    using System.Globalization;

    public static class StringExtension
    {
        public static string FormatInvariant(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args ?? new object[0]);
        }

        public static string FormatCurrentUICulture(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentUICulture, format, args ?? new object[0]);
        }

        public static string Truncate(this string message, int maximumSize)
        {
            return message.Length > maximumSize ? message.Substring(0, maximumSize) + "...(truncated)" : message;
        }
    }
}
