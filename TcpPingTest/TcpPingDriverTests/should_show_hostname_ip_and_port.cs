using System;
using System.Diagnostics.CodeAnalysis;
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
    public class should_show_hostname_ip_and_port
    {
        private static Mock<IDns> fakeDns;
        private static ISocketService fakeSocketService;
        private static TextWriterSpy fakeOutputWriter;
        private static TcpPingDriver tcpPingDriver;

        public Establish given_tcp_ping_driver_with_dns = () =>
        {
            fakeDns = new Mock<IDns>();
            fakeSocketService = new FakeSocketService();
            fakeOutputWriter = new TextWriterSpy();
            tcpPingDriver = new TcpPingDriver(
                fakeDns.Object,
                fakeSocketService,
                fakeOutputWriter,
                TimeSpan.FromSeconds(0.1),
                TimeSpan.FromSeconds(0.1),
                TimeSpan.FromSeconds(0.1));
        };

        public class when_set_with_ip_and_port
        {
            public Because given_hostname_and_port = () =>
            {
                tcpPingDriver.Drive(new[] {"127.0.0.1:80"});
                output = fakeOutputWriter.Output;
            };

            public It should_show_hostname = () => output.ShouldMatch(new Regex(@"to .*127\.0\.0\.1:.*\["));

            public It should_show_port = () => output.ShouldMatch(new Regex(@"to .*:80.*\["));

            public It should_show_ip = () => output.ShouldMatch(new Regex(@"to .*\[127\.0\.0\.1:"));

            private static string output;
        }

        public class when_set_with_hostname_and_port
        {
            public Establish setup_dns =
                () => fakeDns.Setup(d => d.GetHostAddresses("example.com")).Returns(new[] {IPAddress.Parse("1.1.1.1")});

            public Because given_hostname_and_port = () =>
            {
                tcpPingDriver.Drive(new[] { "example.com:443" });
                output = fakeOutputWriter.Output;
            };

            public It should_show_hostname = () => output.ShouldMatch(new Regex(@"to .*example\.com:.*\["));

            public It should_show_port = () => output.ShouldMatch(new Regex(@"to .*:443.*\["));

            public It should_show_ip = () => output.ShouldMatch(new Regex(@"to .*\[1\.1\.1\.1:"));

            private static string output;
        }

        public class when_host_ip_look_up_failed
        {
            public Establish setup_dns = () => fakeDns.Setup(d => d.GetHostAddresses("failed.example.com")).Throws<Exception>();

            public Because lookup_host_ip = () => exception = Catch.Exception(() => tcpPingDriver.Drive(new[] { "failed.example.com:80" }));

            public It should_throw_exception = () => exception.ShouldNotBeNull();

            public It should_give_error_message = () => exception.Message.ShouldEqual("Failed to look up host address for failed.example.com.");

            private static Exception exception;
        }
    }
}