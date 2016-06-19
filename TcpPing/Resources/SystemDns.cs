using System.Net;
using System.Threading.Tasks;
using TcpPing.Interfaces;

namespace TcpPing.Resources
{
    public class SystemDns : IDns
    {
        public IPAddress[] GetHostAddresses(string hostname)
        {
            return Dns.GetHostAddresses(hostname);
        }

        public Task<IPAddress[]> GetHostAddressesAsync(string hostname)
        {
            return Dns.GetHostAddressesAsync(hostname);
        }
    }
}
