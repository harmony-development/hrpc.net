# hrpc.net

a small [hrpc](https://github.com/harmony-development/hrpc) utility library. only supports client-side usage for now

## Documentation

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