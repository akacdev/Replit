using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Replit.GraphQL
{
    public static class API
    {
        public const int PreviewMaxLength = 500;

        public static async Task<HttpResponseMessage> Request
        (
            this HttpClient cl,
            HttpMethod method,
            object obj,
            JsonSerializerOptions options = null)
        => await Request(cl, method, new StringContent(JsonSerializer.Serialize(obj, options), Encoding.UTF8, Constants.JsonContentType));

        public static async Task<HttpResponseMessage> Request
        (
            this HttpClient cl,
            HttpMethod method,
            HttpContent content)
        {
            HttpRequestMessage req = new(method, ReplitGraphQLClient.BaseUri)
            {
                Content = content
            };

            HttpResponseMessage res = await cl.SendAsync(req);

            if (res.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new GraphQLRatelimitException($"Failed to request {req.RequestUri}, too many requests.");
            }
            else if (res.StatusCode != HttpStatusCode.OK)
            {
                throw new GraphQLException(
                    $"Failed to request {req.RequestUri}, received a failure status code: {res.StatusCode}\n" +
                    $"Preview: {await res.GetPreview()}");
            }

            string contentType = res.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrEmpty(contentType))
                throw new GraphQLException($"Failed to request {req.RequestUri}, response is missing a 'Content-Type' header.");
            if (contentType != Constants.JsonContentType)
                throw new GraphQLException($"Failed to request {req.RequestUri}, response is not JSON. Preview: {await res.GetPreview()}");

            return res;
        }

        public static async Task<T> Deseralize<T>(this HttpResponseMessage res, JsonSerializerOptions options = null)
        {
            Stream stream = await res.Content.ReadAsStreamAsync();
            if (stream.Length == 0) throw new GraphQLException("Response content is empty, can't parse as JSON.");

            try
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, options);
            }
            catch (Exception ex)
            {
                throw new GraphQLException(
                    $"Exception while parsing JSON: {ex.GetType().Name} => {ex.Message}\n" +
                    $"Preview: {await res.GetPreview()}"
                    );
            }
        }

        public static async Task<string> GetPreview(this HttpResponseMessage res)
        {
            string text = await res.Content.ReadAsStringAsync();
            return text[..Math.Min(text.Length, PreviewMaxLength)];
        }
    }
}