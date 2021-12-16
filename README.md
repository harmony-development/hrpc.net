# hrpc.net

a small [hrpc](https://github.com/harmony-development/hrpc) utility library. only supports client-side usage for now

## Documentation

### Requests

```cs
var messageRequest = new Message() { content: "Hi!" };

HttpClient client = ...;

// if you need to add headers (for authorization, for example)
client.DefaultRequestHeaders.Add("Authorization", "token");

await client.HrpcUnaryAsync<Message, Empty>("http://localhost:2289/chat.Chat/SendMessage", messageRequest);
```

### Streams

```cs
var stream = new StreamClient<Message>();

// if you need to add headers
var stream = new StreamClient<Message>(new() { { "Authorization", "token" } });

await client.Connect("http://localhost:2289/chat.Chat/StreamMessages", new Chat.Empty());

while (client.State == WebSocketState.Open)
{
    var message = await client.Read();
    _ = HandleMessageReceived(message);
}

Console.WriteLine($"Stream client closed with status {client.CloseStatus}!");
```

### Serializing / Deserializing

```cs
// usage in HTTP requests
byte[] data = ...;
int statusCode = ...;
MyClass parsed = Proto.Deserialize<MyClass>(data, statusCode); // throws HrpcError if statusCode != 200

// usage in streams
byte[] data = ...;
MyClass parsed = Proto.Deserialize<MyClass>(data); // throws HrpcError if serialized message begins with 1 (error)
```

### Errors

Any errors thrown by this library are *subclasses* of HrpcError.

```cs
public class HrpcError : Exception
{
    public readonly string Identifier;
    public readonly string HumanMessage;
    public readonly byte[] Details;
}

public class UnknownHrpcError : HrpcError {}
public class InternalServerError : HrpcError {}

public class RateLimited : HrpcError {
    public readonly uint RetryAfter;
}

public class NotFound : HrpcError {}
public class NotImplemented : HrpcError {}
public class Unavailable : HrpcError {}
public class BadUnaryRequest : HrpcError {}
public class BadStreamingRequest : HrpcError {}
```

## License

See `COPYING` file at the root of the repo.