using System.Net;
using System.Net.Sockets;

namespace Boson;

public enum ChannelAddressFamily
{
    IPv4,
    IPv6
}

public enum ChannelProtocol
{
    Tcp,
    Udp
}

public class Channel : IDisposable
{
    public Socket Socket { get; }

    public Channel(ChannelProtocol protocol, ChannelAddressFamily addressFamily = ChannelAddressFamily.IPv4) : this(new Socket(
        addressFamily switch
        {
            ChannelAddressFamily.IPv4 => AddressFamily.InterNetwork,
            ChannelAddressFamily.IPv6 => AddressFamily.InterNetworkV6,
            _ => throw new ArgumentOutOfRangeException(nameof(addressFamily), addressFamily, null)
        }, 
        protocol switch
        {
            ChannelProtocol.Tcp => SocketType.Stream,
            ChannelProtocol.Udp => SocketType.Dgram,
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        },
        protocol switch
        {
            ChannelProtocol.Tcp => ProtocolType.Tcp,
            ChannelProtocol.Udp => ProtocolType.Udp,
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        }))
    {
    }

    protected Channel(Socket socket)
    {
        Socket = socket;
    }

    public static Result<Channel> Create(ChannelProtocol protocol,
        ChannelAddressFamily addressFamily = ChannelAddressFamily.IPv4) =>
        Result<Channel>.Wrap(() => new(protocol, addressFamily));

    public Result Connect(string host, int port) => Result.Wrap(() => Socket.Connect(host, port));
    public Result Connect(IPAddress address, int port) => Result.Wrap(() => Socket.Connect(address, port));
    public Result Connect(IPAddress[] addresses, int port) => Result.Wrap(() => Socket.Connect(addresses, port));
    public Result Connect(IPEndPoint remoteEndpoint) => Result.Wrap(() => Socket.Connect(remoteEndpoint));

    public Result Bind(string host, int port) => Result.Wrap(() => Socket.Bind(new IPEndPoint(IPAddress.Parse(host), port)));
    public Result Bind(IPAddress address, int port) => Result.Wrap(() => Socket.Bind(new IPEndPoint(address, port)));
    public Result Bind(IPEndPoint localEndpoint) => Result.Wrap(() => Socket.Bind(localEndpoint));

    public Result Listen() => Result.Wrap(Socket.Listen);

    public Result<Channel> Accept() => Result<Channel>.Wrap(() =>
    {
        var socket = Socket.Accept();

        //if (socket.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
        //if (socket.SocketType is not (SocketType.Stream or SocketType.Dgram))

        return new Channel(socket);
    });

    public Result<int> Receive(Span<byte> buffer)
    {
        try
        {
            return new Ok<int>(Socket.Receive(buffer));
        }
        catch (Exception ex)
        {
            return new Error<int>(ex);
        }
    }

    public Result Send(ReadOnlySpan<byte> buffer)
    {
        try
        {
            int bytesSent = 0;
            while (bytesSent < buffer.Length)
            {
                int currentBytesSent = Socket.Send(buffer);

                buffer = buffer[currentBytesSent..];
                bytesSent += currentBytesSent;
            }

            return new Ok();
        }
        catch (Exception ex)
        {
            return new Error(ex);
        }
    }

    public void Dispose()
    {
        Socket.Dispose();
    }
}