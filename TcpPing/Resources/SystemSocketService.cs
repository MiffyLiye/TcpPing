using System.Net.Sockets;
using TcpPing.Interfaces;

namespace TcpPing.Resources
{
    public class SystemSocketService : ISocketService
    {
        public ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return new SystemSocket(addressFamily, socketType, protocolType);
        }
    }
}
