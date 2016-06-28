using System;
using TcpPing.Drivers;
using TcpPing.Resources;

namespace TcpPing
{
    public class Program
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