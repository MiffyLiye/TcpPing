using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TcpPing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Drive(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Drive(IReadOnlyList<string> args)
        {
            if (args[0].Split(':').Length != 2)
            {
                throw new ArgumentException("Please specify hostname and port in hostname:port format.");
            }

            var hostname = args[0].Split(':')[0];
            var ip = GetEndPointIp(hostname);

            var port = GetPortNumber(args[0].Split(':')[1]);

            var endPoint = new IPEndPoint(ip, port);

            Console.WriteLine($"TCP connect to {hostname}:{port} [{ip}:{port}]:");

            TcpPing(endPoint, WarmUpLimit);
            var pingResults = new List<double?>();
            for (var i = 0; i < 4; i++)
            {
                var delay = TcpPing(endPoint, TimeOutLimit);
                pingResults.Add(delay);
                Console.WriteLine(
                    delay.HasValue
                        ? $"Connecting to {ip}:{port}: time = {delay.Value:0.00} ms"
                        : "Request timed out");
                Thread.Sleep(RetryInterval);
            }

            Console.WriteLine();
            Console.WriteLine($"TCP connect statistics for {ip}:{port}:");

            var sentCount = pingResults.Count;
            var receivedCount = pingResults.Count(t => t.HasValue);
            var lossCount = sentCount - receivedCount;
            var lossRate = (double) lossCount / sentCount;
            Console.Write($"Sent = {sentCount}, ");
            Console.Write($"Received = {receivedCount}, ");
            Console.Write($"Loss = {lossCount} ");
            Console.WriteLine($"({100 * lossRate:0}% loss)");

            if (!pingResults.Any(t => t.HasValue)) return;

            var validDelays = pingResults.Where(t => t.HasValue).Select(t => t.Value).ToList();
            Console.Write($"Minimum = {validDelays.Min():0.00} ms, ");
            Console.Write($"Maximum = {validDelays.Max():0.00} ms, ");
            Console.WriteLine($"Average = {validDelays.Average():0.00} ms");
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static double? TcpPing(IPEndPoint endPoint, TimeSpan timeOutLimit)
        {
            using (
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    Blocking = true
                })
            {
                var stopWatch = new Stopwatch();

                stopWatch.Start();
                // ReSharper disable once AccessToDisposedClosure
                var taskConnect = Task.Run(() => socket.Connect(endPoint));
                var taskCountDown = Task.Run(() => Thread.Sleep(timeOutLimit));
                Task.WaitAny(taskConnect, taskCountDown);
                stopWatch.Stop();
                socket.Close();

                var delay = stopWatch.Elapsed;

                return delay < timeOutLimit ? (double?) delay.TotalMilliseconds : null;
            }
        }

        private static IPAddress GetEndPointIp(string hostname)
        {
            IPAddress ip = null;
            if (IPAddress.TryParse(hostname, out ip)) return ip;

            try
            {
                ip = Dns.GetHostAddresses(hostname).First();
            }
            catch (Exception)
            {
                throw new ArgumentException($"Failed to look up host address for {hostname}.");
            }
            return ip;
        }

        private static int GetPortNumber(string portString)
        {
            try
            {
                var port = ushort.Parse(portString);
                return port;
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid port number {portString}.");
            }
        }

        private static TimeSpan TimeOutLimit => TimeSpan.FromSeconds(2);

        private static TimeSpan RetryInterval => TimeSpan.FromSeconds(1);

        private static TimeSpan WarmUpLimit => TimeSpan.FromSeconds(0.1);
    }
}