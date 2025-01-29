using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;

namespace BookCatalogApi.CustomMiddlewares;

public class EtagMiddleware
{
    private readonly RequestDelegate _next;

    public EtagMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "GET")
        {

            var responce = context.Response;
            var originalStream = responce.Body;

            using var ms = new MemoryStream();
            responce.Body = ms;

            await _next(context);

            if (IsEtagSupported(responce))
            {
                string checksum = CalculateHash(ms);

                if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etag))
                {
                    responce.StatusCode = StatusCodes.Status304NotModified;
                    return;
                }
                responce.Headers[HeaderNames.ETag] = checksum;
            }
            ms.Position = 0;
            await ms.CopyToAsync(originalStream);
        }
        else
        {
            await _next(context);
        }
    }
    private static bool IsEtagSupported(HttpResponse response)
    {
        if (response.StatusCode != StatusCodes.Status200OK)
        {
            return false;
        }
        if (response.Body.Length > 20 * 1024)
        {
            return false;
        }
        if (response.Headers.ContainsKey(HeaderNames.ETag))
        {
            return false;
        }
        return true;
    }
    private static string CalculateHash(MemoryStream ms)
    {
        string checksum = "";

        using (var algo = SHA1.Create())
        {
            ms.Position = 0;
            byte[] bytes = algo.ComputeHash(ms);
            checksum = $"\"{WebEncoders.Base64UrlEncode(bytes)}\"";
        }
        return checksum;
    }
}
