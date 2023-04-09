// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using SquidEyes.Fundamentals;

namespace SquidEyes.ContractSender;

public static class MiscExtenders
{
    public static Email? ToEmailOrNull(this string value) =>
        value.Get(v => v == null ? default : Email.From(v));

    public static Uri? ToUriOrNull(this string value) =>
        value.Get(v => v == null ? null : new Uri(v));

    public static Email[]? ToEmailArrayOrNull(this string value)
    {
        return value.Get(v => v == null ? default
            : v.Split(';').Select(e => Email.From(e)).ToArray());
    }
}