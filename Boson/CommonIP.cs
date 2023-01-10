using System.Net;
using System.Net.Sockets;

namespace Boson;

public static class CommonIP
{
    public static IPAddress? Localhost
    {
        get
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);
            var endPoint = socket.LocalEndPoint as IPEndPoint;

            return endPoint?.Address;
        }
    }
}