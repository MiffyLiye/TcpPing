using System;
using TcpPing.Drivers;
using TcpPing.Resources;

namespace TcpPing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tcpPingDriver = new TcpPingDriver(
                new SystemDns(),
                new SystemSocketService(),
                Console.Out,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2));

            try
            {
                tcpPingDriver.Drive(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}