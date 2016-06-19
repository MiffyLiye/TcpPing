using System.Net;
using System.Threading.Tasks;

namespace TcpPing.Interfaces
{
    public interface IDns
    {
        IPAddress[] GetHostAddresses(string hostname);
        Task<IPAddress[]> GetHostAddressesAsync(string hostname);
    }
}
