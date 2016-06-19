using System.Net.Sockets;

namespace TcpPing.Interfaces
{
    public interface ISocketService
    {
        ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }
}