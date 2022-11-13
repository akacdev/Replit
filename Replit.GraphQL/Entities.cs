using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Replit.GraphQL
{
    public class GraphQLContainer
    {
        [JsonPropertyName("errors")]
        public GraphQLError[] Errors { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }

    /// <summary>
    /// A server-side error occured when processing the query.
    /// </summary>
    public class GraphQLError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("path")]
        public string[] Paths { get; set; }

        [JsonPropertyName("locations")]
        public Location[] Locations { get; set; }

        [JsonPropertyName("extensions")]
        public Extensions Extensions { get; set; }
    }

    public class Extensions
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }

    /// <summary>
    /// The location in your query where the error has occured.
    /// </summary>
    public class Location
    {
        [JsonPropertyName("line")]
        public int Line { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }
    }

    /// <summary>
    /// Parameters to be passed to the GraphQL API.
    /// </summary>
    public class GraphQLParameters
    {
        public GraphQLParameters(string query, Dictionary<string, object> variables, string operationName = null)
        {
            Query = query;
            Variables = variables;
            OperationName = operationName;
        }

        public GraphQLParameters() { }

        [JsonPropertyName("operationName")]
        public string OperationName { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("variables")]
        public Dictionary<string, object> Variables { get; set; }
    }
}