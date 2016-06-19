using System;
using System.Net;
using System.Threading;
using TcpPing.Interfaces;

namespace TcpPingTest.Resources
{
    public class FakeSocket : ISocket
    {
        private readonly TimeSpan _delay;

        public bool Blocking { get; set; }

        public FakeSocket(int delayInMilliseconds = 0)
        {
            _delay = TimeSpan.FromMilliseconds(delayInMilliseconds);
        }

        public FakeSocket(TimeSpan delay)
        {
            _delay = delay;
        }

        public void Connect(EndPoint endPoint)
        {
            Thread.Sleep(_delay);
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}