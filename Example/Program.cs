using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Replit.GraphQL;
using System.Linq;

namespace Example
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Enter your Replit connect.sid cookie value or press enter to skip:");
            string sid = Console.ReadLine();
            if (string.IsNullOrEmpty(sid)) sid = null;

            ReplitGraphQLClient client = new("Example Application", sid);

            string userByUsername = @"
                query userByUsername($username: String!) {
                    user: userByUsername(username: $username) {
                        id
                        bio
                        firstName
                        lastName
                        timeCreated
                }
            }";

            string replByUrl = @"
                query ReplByURL($url: String!) {
                    repl(url: $url) {
                      ... on Repl {
                        id
                        url
                        title              
	                    slug
                        description
                        timeCreated
                    }
                }
            }";

            Console.WriteLine("> Executing a single query: 'userByUsername'");

            User user = (await client.Execute<UserContainer>(userByUsername, new Dictionary<string, object>()
            {
                {
                    "username", "akac"
                }
            }))?.User;

            if (user is null) Console.WriteLine("The provided user doesn't exist.");
            else
            {
                Console.WriteLine($"Name: {user.FirstName} {user.LastName}");
                Console.WriteLine($"ID: {user.Id}");
                Console.WriteLine($"Bio: {user.Bio.Replace("\n", ", ")}");
                Console.WriteLine($"Registered At: {user.TimeCreated}");
            }

            Console.WriteLine("> Executing multiple queries: 'replByUrl'");

            Repl[] repls = (await client.BulkExecute<ReplContainer>(new GraphQLParameters[]
            {
                new(replByUrl, new() { { "url", "https://replit.com/@amasad/TroubledPersonalBaitware" } }),
                new(replByUrl, new() { { "url", "https://replit.com/@amasad/my-fun-new-app" } }),
                new(replByUrl, new() { { "url", "https://replit.com/@amasad/comic-sans" } })
            }))?.Select(data => data.Repl).ToArray();

            Console.WriteLine($"Retrieved {repls.Length} repls back");

            foreach (Repl repl in repls) Console.WriteLine($"=> {repl.Title} ({repl.Id}) was created at {repl.TimeCreated}");

            Console.WriteLine("Demo finished!");
            Console.ReadKey();
        }
    }
}