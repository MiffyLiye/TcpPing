using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Machine.Specifications;
using Moq;
using TcpPing.Drivers;
using TcpPing.Interfaces;
using TcpPingTest.Resources;
using It = Machine.Specifications.It;

namespace TcpPingTest.TcpPingDriverTests
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class should_show_each_ping_time_and_statistics
    {
        private static Mock<IDns> fakeDns;
        private static FakeSocketService fakeSocketService;
        private static TextWriterSpy fakeOutputWriter;
        private static TcpPingDriver tcpPingDriver;

        private static TimeSpan RetryInterval => TimeSpan.FromSeconds(0.1);
        private static TimeSpan TimeOutLimit => TimeSpan.FromSeconds(0.1);
        private static string NewLine => Environment.NewLine;

        public Establish given_tcp_ping_driver_with_dns = () =>
        {
            fakeDns = new Mock<IDns>();
            fakeSocketService = new FakeSocketService();
            fakeOutputWriter = new TextWriterSpy();
            tcpPingDriver = new TcpPingDriver(
                fakeDns.Object,
                fakeSocketService,
                fakeOutputWriter,
                RetryInterval,
                TimeOutLimit);
        };

        public class when_all_pings_received
        {
            public Because ping_remote = () =>
            {
                tcpPingDriver.Drive(new[] {"1.1.1.1:80"});
                output = fakeOutputWriter.Output;
            };

            public It should_show_four_ping_time =
            () => output.Split(new[] {NewLine}, StringSplitOptions.None)
                .Count(line => new Regex(@"time = .* ms").Match(line).Success)
                .ShouldEqual(4);

            public It should_show_packet_loss_rate =
            () => output.ShouldContain(@"Sent = 4, Received = 4, Loss = 0 (0% loss)");

            public It should_show_time_statistics =
            () => output.ShouldMatch(new Regex(@"Minimum = .* ms, Maximum = .* ms, Average = .* ms"));

            private static string output;
        }

        public class when_one_ping_timed_out
        {
            public Establish setup_one_slow_socket_connection_potential = () =>
            {
                var socketForWarmUp = new FakeSocket();
                var delay = TimeSpan.FromSeconds(1);
                var slowSocket = new FakeSocket(delay);
                fakeSocketService.ResetSockets(new List<ISocket> {socketForWarmUp, slowSocket});
                fakeDns.Setup(d => d.GetHostAddresses("example.com")).Returns(new[] {IPAddress.Parse("1.1.1.1")});
            };

            public Because ping_remote = () =>
            {
                tcpPingDriver.Drive(new[] {"example.com:80"});
                output = fakeOutputWriter.Output;
            };

            public It should_show_three_ping_time =
            () => output.Split(new[] {NewLine}, StringSplitOptions.None)
                .Count(line => new Regex(@"time = .* ms").Match(line).Success)
                .ShouldEqual(3);

            public It should_show_one_request_timed_out =
            () => output.Split(new[] {NewLine}, StringSplitOptions.None)
                .Count(line => new Regex(@"Request timed out").Match(line).Success)
                .ShouldEqual(1);

            public It should_show_packet_loss_rate =
            () => output.ShouldContain(@"Sent = 4, Received = 3, Loss = 1 (25% loss)");

            public It should_show_time_statistics =
            () => output.ShouldMatch(new Regex(@"Minimum = .* ms, Maximum = .* ms, Average = .* ms"));

            private static string output;
        }

        public class when_all_pings_timed_out
        {
            public Establish setup_one_slow_socket_connection_potential = () =>
            {
                var socketForWarmUp = new FakeSocket();
                var delay = TimeSpan.FromSeconds(1);
                var slowSocket = new FakeSocket(delay);
                fakeSocketService.ResetSockets(new List<ISocket>
                {
                    socketForWarmUp,
                    slowSocket,
                    slowSocket,
                    slowSocket,
                    slowSocket
                });
                fakeDns.Setup(d => d.GetHostAddresses("localhost")).Returns(new[] {IPAddress.Parse("127.0.0.1")});
            };

            public Because ping_remote = () =>
            {
                tcpPingDriver.Drive(new[] {"localhost:80"});
                output = fakeOutputWriter.Output;
            };

            public It should_show_zero_ping_time =
            () => output.Split(new[] {NewLine}, StringSplitOptions.None)
                .Count(line => new Regex(@"time = .* ms").Match(line).Success)
                .ShouldEqual(0);

            public It should_show_four_request_timed_out =
            () => output.Split(new[] {NewLine}, StringSplitOptions.None)
                .Count(line => new Regex(@"Request timed out").Match(line).Success)
                .ShouldEqual(4);

            public It should_show_packet_loss_rate =
            () => output.ShouldContain(@"Sent = 4, Received = 0, Loss = 4 (100% loss)");

            public It should_not_show_time_statistics =
            () => output.Split(new[] {NewLine}, StringSplitOptions.None)
                .Count(line => new Regex(@"Minimum = .* ms, Maximum = .* ms, Average = .* ms").Match(line).Success)
                .ShouldEqual(0);

            private static string output;
        }
    }
}