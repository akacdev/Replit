using System;

namespace Replit.GraphQL
{
    /// <summary>
    /// Represents an exception used when API ratelimits occur.
    /// </summary>
    public class GraphQLRatelimitException : Exception { public GraphQLRatelimitException(string message) : base(message) { } }

    /// <summary>
    /// Represents an exception used when API requests return a failure status code, or when errors occur server-side.
    /// </summary>
    public class GraphQLException : Exception
    {
        public GraphQLError[] Errors { get; set; } = null;

        public GraphQLException(string message) : base(message) { }
        public GraphQLException(string message, GraphQLError[] errors) : base(message) { Errors = errors; }
    }
}