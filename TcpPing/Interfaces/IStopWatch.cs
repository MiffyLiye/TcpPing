using System;

namespace TcpPing.Interfaces
{
    public interface IStopWatch
    {
        void Start();
        void Stop();
        TimeSpan Elapsed { get; }
    }
}