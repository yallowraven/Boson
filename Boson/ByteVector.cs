namespace Boson;

internal class ByteVector
{
    byte[] buffer;

    internal int Length => buffer.Length;
    internal Span<byte> Buffer => buffer;

    internal ByteVector(int length = 64)
    {
        buffer = new byte[length];
    }

    internal Span<byte> Expand()
    {
        byte[] newBuffer = new byte[buffer.Length * 2];
        System.Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
        buffer = newBuffer;

        return Buffer;
    }
}