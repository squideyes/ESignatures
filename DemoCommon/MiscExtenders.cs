using System.Globalization;

namespace DemoCommon;

public static class MiscExtenders
{
    public static string ToDayName(this DateTime date)
    {
        return date.Day switch
        {
            1 or 21 or 31 => date.Day + "st",
            2 or 22 => date.Day + "nd",
            3 or 23 => date.Day + "rd",
            _ => date.Day + "th"
        };
    }

    public static string ToMonthName(this DateTime date) =>
        CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(date.Month);
}
