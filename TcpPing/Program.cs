using System;
using TcpPing.Drivers;

namespace TcpPing
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var tcpPingDriver = new TcpPingDriver();

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