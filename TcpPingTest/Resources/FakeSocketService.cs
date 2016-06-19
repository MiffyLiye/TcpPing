using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using TcpPing.Interfaces;

namespace TcpPingTest.Resources
{
    public class FakeSocketService : ISocketService
    {
        private readonly Queue<ISocket> _sockets;

        public FakeSocketService()
        {
            _sockets = new Queue<ISocket>();
        }

        public ISocket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return _sockets.Any() ? _sockets.Dequeue() : new FakeSocket();
        }

        public void ResetSockets(IEnumerable<ISocket> sockets)
        {
            this._sockets.Clear();
            foreach (var socket in sockets)
            {
                this._sockets.Enqueue(socket);
            }
        }
    }
}