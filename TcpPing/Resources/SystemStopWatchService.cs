using TcpPing.Interfaces;

namespace TcpPing.Resources
{
    public class SystemStopWatchService : IStopWatchService
    {
        public IStopWatch GetStopWatch()
        {
            return new SystemStopWatch();
        }
    }
}