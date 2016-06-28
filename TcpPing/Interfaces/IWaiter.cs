using System;

namespace TcpPing.Interfaces
{
    public interface IWaiter
    {
        void Wait(TimeSpan timeSpan);
    }
}