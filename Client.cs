using System.Buffers;
using System.Net.WebSockets;

using Google.Protobuf;

namespace Hrpc;

public static class Requests
{
    public static async Task<T> HrpcUnaryAsync<V, T>(this HttpClient client, string url, V value)
        where V : IMessage<V> where T : IMessage<T>, new()
    {
        var content = new ByteArrayContent(Proto.Marshal(value));
        content.Headers.Add("Content-Type", "application/hrpc");

        var res = await client.PostAsync(url, content);

        return Proto.Unmarshal<T>(await res.Content.ReadAsByteArrayAsync(), (int)res.StatusCode);
    }
}

public class StreamClient<T> where T : IMessage<T>, new()
{
    private ClientWebSocket _socket = new();
    public WebSocketState State => _socket.State;
    public int? CloseStatus => (int?)_socket.CloseStatus;

    public StreamClient() { }

    public StreamClient(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
            _socket.Options.SetRequestHeader(header.Key, header.Value);
    }

    public async Task Connect(string url, IMessage initial, CancellationToken token = default)
    {
        await _socket.ConnectAsync(new Uri(url), token);

        await _socket.SendAsync(Proto.Marshal(initial).AsMemory(), WebSocketMessageType.Binary, true, token);
    }

    public async Task<T> Read(CancellationToken token = default)
    {
        var bytesReceived = 0;
        using var stream = new MemoryStream();
        using var buf = MemoryPool<byte>.Shared.Rent();

        var msg = await _socket.ReceiveAsync(buf.Memory, token);
        bytesReceived += msg.Count;
        stream.Write(buf.Memory.Span.Slice(0, msg.Count));

        while (!msg.EndOfMessage)
        {
            msg = await _socket.ReceiveAsync(buf.Memory, token);
            bytesReceived += msg.Count;
            stream.Write(buf.Memory.Span.Slice(0, msg.Count));
        }

        return Proto.Unmarshal<T>(stream.GetBuffer()[..bytesReceived]);
    }
}