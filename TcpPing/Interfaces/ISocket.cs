using System;
using System.Net;

namespace TcpPing.Interfaces
{
    public interface ISocket : IDisposable
    {
        bool Blocking { get; set; }
        void Connect(EndPoint endPoint);
        void Close();
    }
}
