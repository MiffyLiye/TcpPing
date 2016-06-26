using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TcpPing.Interfaces;

namespace TcpPing.Drivers
{
    public class TcpPingDriver
    {
        private IDns Dns { get; }
        private TextWriter OutputWriter { get; }
        private ISocketService SocketService { get; }

        private TimeSpan RetryInterval { get; }
        private TimeSpan TimeOutLimit { get; }
        private int RetryTimes { get; }

        public TcpPingDriver(
            IDns dns,
            ISocketService socketService,
            TextWriter outputWriter,
            TimeSpan retryInterval,
            TimeSpan timeOutLimit,
            int retryTimes = 4)
        {
            Dns = dns;
            SocketService = socketService;
            OutputWriter = outputWriter;
            RetryInterval = retryInterval;
            TimeOutLimit = timeOutLimit;
            RetryTimes = retryTimes;
        }

        private string Hostname { get; set; }
        private int Port { get; set; }
        private IPAddress Ip { get; set; }
        private IPEndPoint RemoteEndPoint { get; set; }

        private static IPEndPoint LocalEndPoint => new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);

        public void Drive(IReadOnlyList<string> args)
        {
            SetRemoteEndPoint(args[0]);
            OutputWriter.WriteLine($"TCP connect to {Hostname}:{Port} [{Ip}:{Port}]:");

            //Trigger JIT before time recording starts, details in issue #2
            TcpPing(LocalEndPoint, TimeOutLimit);

            var pingResults = new List<double?>();
            for (var i = 0; i < RetryTimes; i++)
            {
                var delay = TcpPing(RemoteEndPoint, TimeOutLimit);
                pingResults.Add(delay);
                OutputWriter.WriteLine(
                    delay.HasValue
                        ? $"Connecting to {Ip}:{Port}: time = {delay.Value:0.00} ms"
                        : "Request timed out");
                Thread.Sleep(RetryInterval);
            }

            OutputWriter.WriteLine();
            OutputWriter.WriteLine($"TCP connect statistics for {Ip}:{Port}:");

            var sentCount = pingResults.Count;
            var receivedCount = pingResults.Count(t => t.HasValue);
            var lossCount = sentCount - receivedCount;
            var lossRate = (double) lossCount / sentCount;
            OutputWriter.Write($"Sent = {sentCount}, ");
            OutputWriter.Write($"Received = {receivedCount}, ");
            OutputWriter.Write($"Loss = {lossCount} ");
            OutputWriter.WriteLine($"({100 * lossRate:0}% loss)");

            if (pingResults.All(t => !t.HasValue)) return;

            var validDelays = pingResults.Where(t => t.HasValue).Select(t => t.Value).ToList();
            OutputWriter.Write($"Minimum = {validDelays.Min():0.00} ms, ");
            OutputWriter.Write($"Maximum = {validDelays.Max():0.00} ms, ");
            OutputWriter.WriteLine($"Average = {validDelays.Average():0.00} ms");
        }

        private void SetRemoteEndPoint(string arg)
        {
            if (arg.Split(':').Length != 2)
            {
                throw new ArgumentException("Please specify hostname and port in hostname:port format.");
            }

            Hostname = arg.Split(':')[0];
            Ip = GetEndPointIp(Hostname);
            Port = GetPortNumber(arg.Split(':')[1]);

            RemoteEndPoint = new IPEndPoint(Ip, Port);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private double? TcpPing(IPEndPoint remote, TimeSpan timeOutLimit)
        {
            using (
                var socket = SocketService.CreateSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            )
            {
                socket.Blocking = true;

                var taskConnect = Task.Run(() =>
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    // ReSharper disable once AccessToDisposedClosure
                    socket.Connect(remote);
                    stopWatch.Stop();
                    return stopWatch.Elapsed;
                });
                var taskCountDown = Task.Run(() => Thread.Sleep(timeOutLimit));
                Task.WaitAny(taskConnect, taskCountDown);

                socket.Close();
                return taskConnect.IsCompleted && !taskConnect.IsFaulted ? (double?) taskConnect.Result.TotalMilliseconds : null;
            }
        }

        private IPAddress GetEndPointIp(string hostname)
        {
            IPAddress ip;
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

        private static int GetPortNumber(string port)
        {
            try
            {
                return ushort.Parse(port);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid port number {port}.");
            }
        }
    }
}