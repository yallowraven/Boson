namespace Boson;

public class TcpChannel : Channel
{
    public TcpChannel(ChannelAddressFamily addressFamily = ChannelAddressFamily.IPv4) : base(ChannelProtocol.Tcp, addressFamily)
    {
    }

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