using Protocol = Hrpc.V1;

namespace Hrpc;

public class HrpcError : Exception
{
    public readonly string Identifier;
    public readonly string HumanMessage;
    public readonly byte[] Details;

    public HrpcError(Protocol.Error error)
    {
        Identifier = error.Identifier;
        HumanMessage = error.HumanMessage;
        Details = error.Details.ToByteArray();
    }
}

public class UnknownHrpcError : HrpcError
{
    public UnknownHrpcError(Protocol.Error error) : base(error) { }
}

public class InternalServerError : HrpcError
{
    public InternalServerError(Protocol.Error error) : base(error) { }
}

public class RateLimited : HrpcError
{
    public readonly uint RetryAfter;

    public RateLimited(Protocol.Error error) : base(error)
    {
        var details = Protocol.RetryInfo.Parser.ParseFrom(error.Details);
        RetryAfter = details.RetryAfter;
    }
}

public class NotFound : HrpcError
{
    public NotFound(Protocol.Error error) : base(error) { }
}

public class NotImplemented : HrpcError
{
    public NotImplemented(Protocol.Error error) : base(error) { }
}

public class Unavailable : HrpcError
{
    public Unavailable(Protocol.Error error) : base(error) { }
}

public class BadUnaryRequest : HrpcError
{
    public BadUnaryRequest(Protocol.Error error) : base(error) { }
}

public class BadStreamingRequest : HrpcError
{
    public BadStreamingRequest(Protocol.Error error) : base(error) { }
}

internal static class ErrorsExt
{
    internal static HrpcError ParseError(Protocol.Error error)
    {
        switch (error.Identifier)
        {
            case "hrpc.internal-server-error":
                return new InternalServerError(error);
            case "hrpc.resource-exhausted":
                return new RateLimited(error);
            case "hrpc.not-found":
                return new NotFound(error);
            case "hrpc.not-implemented":
                return new NotImplemented(error);
            case "hrpc.unavailable":
                return new Unavailable(error);
            case "hrpc.http.bad-unary-request":
                return new BadUnaryRequest(error);
            case "hrpc.http.bad-streaming-request":
                return new BadStreamingRequest(error);
            default:
                return new UnknownHrpcError(error);
        }
    }
}