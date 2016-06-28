using System;
using System.Threading;
using TcpPing.Interfaces;

namespace TcpPing.Resources
{
    public class Waiter : IWaiter
    {
        public void Wait(TimeSpan timeSpan)
        {
            Thread.Sleep(timeSpan);
        }
    }
}