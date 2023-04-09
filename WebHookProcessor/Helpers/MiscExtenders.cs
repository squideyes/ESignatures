// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text;

namespace WebHookProcessor;

internal static class MiscExtenders
{
    public static HttpResponseData GetResponse(
        this HttpRequestData request, HttpStatusCode statusCode, string message)
    {
        var response = request.CreateResponse(statusCode);

        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString(message);

        return response;
    }

    public static string DecodeBase64(this string value) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(value));
}