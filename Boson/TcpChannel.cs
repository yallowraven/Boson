using System.Net.Sockets;

namespace Boson;

public class TcpChannel : Channel
{
    public TcpChannel(ChannelAddressFamily addressFamily = ChannelAddressFamily.IPv4) : base(ChannelProtocol.Tcp, addressFamily)
    { }

    protected TcpChannel(Socket socket) : base(socket)
    {
        //TODO: better exceptions
        if (socket.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
            throw new Exception(
                $"A TcpChannel cannot be created from a Socket with address family {socket.AddressFamily}.");

        if (socket.SocketType is not SocketType.Stream)
            throw new Exception(
                $"A TcpChannel cannot be created from a Socket with socket type {socket.SocketType}.");

        if (socket.ProtocolType is not ProtocolType.Tcp)
            throw new Exception(
                $"A TcpChannel cannot be created from a Socket with protocol type {socket.ProtocolType}.");
    }

    public static Result<TcpChannel> Create(ChannelAddressFamily addressFamily = ChannelAddressFamily.IPv4) =>
        Result<TcpChannel>.Wrap(() => new(addressFamily));

    public new Result<TcpChannel> Accept() => Result<TcpChannel>.Wrap(() => new(Socket.Accept()));

    public Result ReceiveSizedBlock(Span<byte> block)
    {
        try
        {
            var buffer = block;
            int bytesReceived = 0;

            while (bytesReceived < block.Length)
            {
                int currentBytesReceived = Socket.Receive(buffer);
                bytesReceived += currentBytesReceived;

                buffer = buffer[currentBytesReceived..];
            }

            return new Ok();
        }
        catch (Exception ex)
        {
            return new Error(ex);
        }
    }

    public Result ReceiveBlock(out ReadOnlySpan<byte> block)
    {
        try
        {
            byte[] lengthBytes = new byte[4];
            ReceiveSizedBlock(lengthBytes).Unwrap();

            int blockLength = BitConverter.ToInt32(lengthBytes);
            byte[] blockBytes = new byte[blockLength];
            ReceiveSizedBlock(blockBytes).Unwrap();

            block = blockBytes;
            return new Ok();
        }
        catch (Exception ex)
        {
            block = Span<byte>.Empty;
            return new Error(ex);
        }
    }

    public Result SendBlock(ReadOnlySpan<byte> buffer)
    {
        try
        {
            Send(BitConverter.GetBytes(buffer.Length)).Unwrap();
            Send(buffer).Unwrap();

            return new Ok();
        }
        catch (Exception ex)
        {
            return new Error(ex);
        }
    }
}