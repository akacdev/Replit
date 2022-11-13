using System;
using System.Text.Json.Serialization;

namespace Example
{
    ///Basic classes to deserialize a 'userByUsername' query
    public class UserContainer
    {
        [JsonPropertyName("user")]
        public User User { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("timeCreated")]
        public DateTime TimeCreated { get; set; }
    }


    ///Basic classes to deserialize a  'replByUrl' query
    public class ReplContainer
    {
        [JsonPropertyName("repl")]
        public Repl Repl { get; set; }
    }

    public class Repl
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("timeCreated")]
        public DateTime TimeCreated { get; set; }
    }
}