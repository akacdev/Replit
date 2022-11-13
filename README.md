# Replit

<div align="center">
  <img width="192" height="192" src="https://raw.githubusercontent.com/actually-akac/Replit/master/Replit.GraphQL/icon.png">
</div>

<div align="center">
  An async and lightweight C# library for interacting with Replit APIs.
</div>

## Usage
Provides an easy interface for interacting with Replit APIs. Currently distributed packages:
* `Replit.GraphQL` ([NuGet](https://www.nuget.org/packages/Replit.GraphQL)) ([GitHub](https://github.com/actually-akac/Replit/tree/master/Replit.GraphQL)) - Wrapper for the GraphQL API.

To get started, add the library into your solution with either the `NuGet Package Manager` or the `dotnet` CLI.
```rust
dotnet add package Replit.GraphQL
```

For the primary classes to become available, import the used namespace.
```csharp
using Replit.GraphQL;
```

> **Warning**<br>
> Certain actions require you to pass a valid **session identifier** (`connect.sid`) cookie. You can obtain this from the browser `Developer Tools` while logged in.<br>
> Unsure of how to obtain this value? See [this guide](https://replit.com/talk/learn/How-to-Get-Your-SID-Cookie/145979) written by [RayhanADev](https://replit.com/@RayhanADev).<br>
> If you are on mobile, you can alternatively use [this web based form](https://extract-sid.ironcladdev.repl.co) made by [IroncladDev](https://replit.com/@IroncladDev).

## Features
- Built for `.NET 6` and `.NET 7`
- Fully **async**
- Extensive **XML documentation**
- **No external dependencies** (uses integrated HTTP and JSON)
- **Custom exceptions** for advanced catching
- Execute **single** or **bulk** Replit GraphQL queries

## Example
Under the `Example` directory you can find a working demo project that implements this library.

## Code Samples

### Initializing a new GraphQL client with a valid session identifier cookie
```csharp
string sid = "s%3Bew5ttAQuBlAANjIh8ABSoTTtZ75-7AbH.ohRBI9wNPwOHED7GLltPBrOS975gxqATe1aL6y%9N3%2Fla";
ReplitGraphQLClient client = new(sid, "Example Application");
```

### Executing a single query
```csharp
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

User user = (await client.Execute<UserContainer>(userByUsername, new Dictionary<string, object>()
{
    {
        "username", "akac"
    }
}))?.User;
```

### Executing bulk queries
```csharp
Repl[] repls = (await client.BulkExecute<ReplContainer>(new GraphQLParameters[]
{
    new(replByUrl, new() { { "url", "https://replit.com/@amasad/TroubledPersonalBaitware" } }),
    new(replByUrl, new() { { "url", "https://replit.com/@amasad/my-fun-new-app" } }),
    new(replByUrl, new() { { "url", "https://replit.com/@amasad/comic-sans" } })
}))?.Select(data => data.Repl).ToArray();

Console.WriteLine($"Retrieved {repls.Length} repls back");

foreach (Repl repl in repls) Console.WriteLine($"=> {repl.Title} ({repl.Id}) was created at {repl.TimeCreated}");
```

## Available Methods

- Task\<GraphQLContainer[]> **BulkExecuteRaw**(GraphQLParameters[] parameters, JsonSerializerOptions options = null)
- Task\<GraphQLContainer> **ExecuteRaw**(string query, Dictionary<string, object> variables = null, string operationName = null, JsonSerializerOptions options = null)
- Task\<T[]> **BulkExecute<T>**(GraphQLParameters[] parameters, bool suppressErrors = false, JsonSerializerOptions options = null)
- Task\<T> **Execute<T>**(string query, Dictionary<string, object> variables = null, bool suppressErrors = false, string operationName = null, JsonSerializerOptions options = null)

## References
- https://replit.com
- https://graphql.org