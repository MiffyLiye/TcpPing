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
                Driver(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Driver(IReadOnlyList<string> args)
        {
            if (args[0].Split(':').Length != 2)
            {
                throw new ArgumentException("Please specify hostname and port in hostname:port format.");
            }

            var hostname = args[0].Split(':')[0];
            var ip = GetEndPointIp(hostname);

            var port = GetPortNumber(args[0].Split(':')[1]);

            var endPoint = new IPEndPoint(ip, port);

            Console.WriteLine(
                $"Pinging {hostname}:{port} [{ip}:{port}]:");

            var pingResults = new List<double?>();

            WarmUp(endPoint);
            foreach (var delay in TcpPing(endPoint))
            {
                pingResults.Add(delay);
                Console.WriteLine(
                    delay.HasValue
                        ? $"time = {delay.Value:0.00} ms"
                        : "Request timed out");
            }

            var validDelays = pingResults
                .Where(t => t.HasValue)
                .Select(t => t.Value)
                .ToList();
            Console.WriteLine(
                $"--- {ip}:{port} ping statistics ---");

            if (validDelays.Any())
                Console.WriteLine(
                    $"round-trip min/avg/max = {validDelays.Min():0.00}/{validDelays.Max():0.00}/{validDelays.Average():0.00} ms");

            var sentCount = pingResults.Count;
            var receivedCount = pingResults.Count(t => t.HasValue);
            var lossCount = sentCount - receivedCount;
            var lossRate = lossCount / sentCount;
            if (lossCount > 0)
                Console.WriteLine(
                    $"{sentCount} packets transmitted, {receivedCount} packets received, {100 * lossRate:0.0}% packet loss");
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static IEnumerable<double?> TcpPing(IPEndPoint endPoint)
        {
            for (var i = 0; i < 4; i++)
            {
                using (var socket =
                        new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            Blocking = true
                        }
                )
                {
                    var stopWatch = new Stopwatch();

                    stopWatch.Start();
                    // ReSharper disable once AccessToDisposedClosure
                    var taskConnect = Task.Run(() => socket.Connect(endPoint));
                    var taskCountDown = Task.Run(() => Thread.Sleep(TimeOutLimit));
                    Task.WaitAny(taskConnect, taskCountDown);
                    stopWatch.Stop();
                    socket.Close();

                    var delay = stopWatch.Elapsed;

                    yield return delay < TimeOutLimit ? (double?) delay.TotalMilliseconds : null;

                    Thread.Sleep(Interval);
                }
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void WarmUp(IPEndPoint endPoint)
        {
            using (var socket =
                    new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        Blocking = true
                    }
            )
            {
                var stopWatch = new Stopwatch();

                stopWatch.Start();
                // ReSharper disable once AccessToDisposedClosure
                var taskConnect = Task.Run(() => socket.Connect(endPoint));
                var taskCountDown = Task.Run(() => Thread.Sleep(WarmUpLimit));
                Task.WaitAny(taskConnect, taskCountDown);
                stopWatch.Stop();
                socket.Close();
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
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid port number {portString}.");
            }
        }

        private static TimeSpan TimeOutLimit => TimeSpan.FromSeconds(2);

        private static TimeSpan Interval => TimeSpan.FromSeconds(1);

        private static TimeSpan WarmUpLimit => TimeSpan.FromSeconds(0.1);
    }
}