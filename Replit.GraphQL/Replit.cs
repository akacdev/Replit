using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Replit.GraphQL
{
    /// <summary>
    /// The primary class for interacting with the GraphQL API.
    /// </summary>
    public class ReplitGraphQLClient
    {
        /// <summary>
        /// The base URL to use when communicating.
        /// </summary>
        public const string BaseUrl = "https://replit.com/graphql";

        /// <summary>
        /// The base URI to use when communicating.
        /// </summary>
        public static readonly Uri BaseUri = new(BaseUrl);

        /// <summary>
        /// The HTTP request version to use when communicating.
        /// </summary>s
        public static readonly Version HttpVersion = new(2, 0);

        /// <summary>
        /// The maximum duration to wait for a response from the server.
        /// </summary>
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private readonly HttpClient Client = new(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.All
        });

        /// <summary>
        /// Initialize a new instance of the Replit GraphQL client.
        /// </summary>
        /// <param name="identity">A short string identifying you and your application. When left on <see langword="null"/>, your current entry assembly name is used.</param>
        /// <param name="sid">Your Replit <b>session ID</b>. Seee the Github repository for instructions on obtaining this value.</param>
        public ReplitGraphQLClient(string identity = null, string sid = null)
        {
            identity = $"Replit.NET/GraphQL - actually-akac/Replit | {identity ?? Assembly.GetEntryAssembly().GetName().Name}";
            
            Client.BaseAddress = BaseUri;
            Client.DefaultRequestVersion = HttpVersion;
            Client.Timeout = Timeout;

            Client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            Client.DefaultRequestHeaders.UserAgent.ParseAdd(identity);
            Client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            Client.DefaultRequestHeaders.Add("X-Requested-With", "Replit.NET");
            Client.DefaultRequestHeaders.Add("Origin", "https://replit.com");

            if (!string.IsNullOrEmpty(sid))
            {
                if (sid.StartsWith("s:")) sid = WebUtility.UrlEncode(sid);
                Client.DefaultRequestHeaders.Add("Cookie", $"connect.sid={sid}");
            }
        }

        private static GraphQLException ParseErrors(GraphQLError[] errors)
        {
            StringBuilder sb = new();
            sb.AppendLine($"Failed to execute query, received {errors.Length} API error{(errors.Length > 1 ? "s" : "")}.");

            for (int i = 0; i < errors.Length; i++)
            {
                GraphQLError error = errors[i];

                sb.AppendLine(
                    $"[{i}] - " +
                    ((error.Extensions is null || error.Extensions.Code is null) ? "" : $"({error.Extensions.Code}) ") +
                    error.Message +
                    ((error.Paths is null || error.Paths.Length == 0) ? "" : $" at {string.Join(", ", error.Paths)}") +
                    ((error.Locations is null || error.Locations.Length == 0) ? "" : $" ({string.Join("; ", error.Locations.Select(loc => $"{loc.Line}: {loc.Column}"))})")
                    );
            }

            return new GraphQLException(sb.ToString(), errors);
        }

        /// <summary>
        /// Execute a single GraphQL query using the API.
        /// </summary>
        /// <typeparam name="T">The type of the <c>data</c> property to deserialize into.</typeparam>
        /// <param name="query">The GraphQL query to request.</param>
        /// <param name="variables">The GraphQL variables to pass to the request.</param>
        /// <param name="suppressErrors">When <see langword="true"/>, <see langword="null"/> is returned insteaad of a <see cref="GraphQLException"/>.</param>
        /// <param name="operationName">Optional GraphQL operation name to execute.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to pass into the JSON deserializer.</param>
        /// <returns>The query results, or <see langword="null"/> when <c>suppressErrors</c> is enabled.</returns>
        /// <exception cref="GraphQLException"></exception>
        /// <exception cref="GraphQLRatelimitException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<T> Execute<T>(
            string query, Dictionary<string, object> variables = null, bool suppressErrors = false, string operationName = null, JsonSerializerOptions options = null
        ) where T : class
        {
            GraphQLContainer container = await ExecuteRaw(query, variables, operationName, options);
            if (container.Errors is not null)
            {
                if (suppressErrors) return null;
                throw ParseErrors(container.Errors);
            }

            return container.Data.Deserialize<T>(options);
        }

        /// <summary>
        /// Execute a single GraphQL query using the API, but skip deserialization into the target type.
        /// </summary>
        /// <param name="query">The GraphQL query to request.</param>
        /// <param name="variables">The GraphQL variables to pass to the request.</param>
        /// <param name="operationName">Optional GraphQL operation name to execute.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to pass into the JSON deserializer.</param>
        /// <exception cref="GraphQLException"></exception>
        /// <exception cref="GraphQLRatelimitException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<GraphQLContainer> ExecuteRaw(
            string query, Dictionary<string, object> variables = null, string operationName = null, JsonSerializerOptions options = null
        )
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query), "Query is null or empty.");

            HttpResponseMessage res = await Client.Request(HttpMethod.Post, new GraphQLParameters()
            {
                Query = query,
                Variables = variables,
                OperationName = operationName
            }, options);

            return await res.Deseralize<GraphQLContainer>(options);
        }

        /// <summary>
        /// Execute multiple GraphQL queries using the API.
        /// </summary>
        /// <typeparam name="T">The type of the <c>data</c> property to deserialize into.</typeparam>
        /// <param name="parameters">An array of GraphQL parameter objects.</param>
        /// <param name="suppressErrors">When <see langword="true"/>, <see langword="null"/> is returned insteaad of a <see cref="GraphQLException"/>.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to pass into the JSON deserializer.</param>
        /// <returns>The query results, or <see langword="null"/> when <c>suppressErrors</c> is enabled.</returns>
        /// <exception cref="GraphQLException"></exception>
        /// <exception cref="GraphQLRatelimitException"></exception>
        /// <exception cref="AggregateException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<T[]> BulkExecute<T>(
            GraphQLParameters[] parameters, bool suppressErrors = false, JsonSerializerOptions options = null
        ) where T : class
        {
            GraphQLContainer[] containers = await BulkExecuteRaw(parameters, options);
            if (!suppressErrors && containers.Any(con => con.Errors is not null))
            {
                List<GraphQLException> exceptions = new();

                foreach (GraphQLContainer container in containers)
                {
                    if (container.Errors is null) continue;
                    exceptions.Add(ParseErrors(container.Errors));
                }

                throw new AggregateException(exceptions);
            }

            return (suppressErrors ? containers.Where(cont => cont.Errors is null) : containers)
                        .Select(cont => cont.Data.Deserialize<T>(options)).ToArray();
        }

        /// <summary>
        /// Execute multiple GraphQL queries using the API, but skip deserialization into the target type.
        /// </summary>
        /// <param name="parameters">An array of GraphQL parameter objects.</param>
        /// <param name="options">Optional <see cref="JsonSerializerOptions"/> to pass into the JSON deserializer.</param>
        /// <exception cref="GraphQLException"></exception>
        /// <exception cref="GraphQLRatelimitException"></exception>
        /// <exception cref="AggregateException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<GraphQLContainer[]> BulkExecuteRaw(
            GraphQLParameters[] parameters, JsonSerializerOptions options = null
        )
        {
            if (parameters is null) throw new ArgumentNullException(nameof(parameters), "GraphQL parameters are null.");
            if (parameters.Length == 0) throw new ArgumentNullException(nameof(parameters), "GraphQL parameter array is empty.");
            if (parameters.Length > 100) throw new ArgumentOutOfRangeException(nameof(parameters), $"You cannot execute more than 100 queries in one request. Current length: {parameters.Length}.");

            HttpResponseMessage res = await Client.Request(HttpMethod.Post, parameters, options);

            return await res.Deseralize<GraphQLContainer[]>(options);
        }
    }
}