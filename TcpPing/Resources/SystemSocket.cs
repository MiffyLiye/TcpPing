using System.Net.Sockets;
using TcpPing.Interfaces;

namespace TcpPing.Resources
{
    public class SystemSocket : Socket, ISocket
    {
        public SystemSocket(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
        {
        }

        public SystemSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }

        public SystemSocket(SocketInformation socketInformation) : base(socketInformation)
        {
        }
    }
}
