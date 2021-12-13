using Protocol = Hrpc.V1;

using Google.Protobuf;

namespace Hrpc;

public static class Proto
{
    private static Dictionary<string, MessageParser> _parser = new();

    public static byte[] Marshal(this IMessage message) => message.ToByteArray();

    public static T Unmarshal<T>(this byte[] message, int statusCode) where T : IMessage<T>, new()
        => UnmarshalInner<T>(message, statusCode == 200);

    public static T Unmarshal<T>(this byte[] message) where T : IMessage<T>, new()
    {
        bool success = message.Take(1).First() == 0;
        var msg = message.Skip(1).ToArray();
        return UnmarshalInner<T>(msg, success);
    }

    private static T UnmarshalInner<T>(this byte[] message, bool ok) where T : IMessage<T>, new()
    {
        if (!ok)
            throw ErrorsExt.ParseError(Protocol.Error.Parser.ParseFrom(message));

        var type = typeof(T).ToString();
        if (_parser.ContainsKey(type))
            return (T)_parser[type].ParseFrom(message);
        else
        {
            _parser.Add(type, new MessageParser<T>(() => new T()));
            return UnmarshalInner<T>(message, ok);
        }
    }
}
