using System.Net.Http.Headers;
using System.Text.Json;
using IntegrationTests.Identity.Avatars;

namespace IntegrationTests.Identity.Api;

internal static class AvatarRequests
{
    public static MultipartFormDataContent Image(int width = 600, int height = 400) =>
        File(TestImages.Create(width, height), "image/jpeg");

    public static MultipartFormDataContent File(byte[] bytes, string contentType)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", "photo" } };
    }

    public static async Task<string?> ReadStringAsync(HttpResponseMessage response, string property)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty(property).GetString();
    }
}
