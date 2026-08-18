namespace TaskTracker.Business.Extensions;

using System;
using System.Globalization;

public static class DateOnlyExtensions
{
    /// <summary>
    /// Returns the date of the start of the week (default is Monday).
    /// </summary>
    public static DateOnly StartOfWeek(this DateOnly date, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff);
    }

    /// <summary>
    /// Returns the date of the end of the week (default is Sunday).
    /// </summary>
    public static DateOnly EndOfWeek(this DateOnly date, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        return date.StartOfWeek(startOfWeek).AddDays(6);
    }

    /// <summary>
    /// Alternative method to get the start of the week based on the current culture's first day of the week.
    /// </summary>
    public static DateOnly StartOfCurrentCultureWeek(this DateOnly date)
    {
        DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        return date.StartOfWeek(firstDayOfWeek);
    }
}