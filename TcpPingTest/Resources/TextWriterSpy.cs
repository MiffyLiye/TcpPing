using System.IO;
using System.Text;

namespace TcpPingTest.Resources
{
    public class TextWriterSpy : TextWriter
    {
        private readonly StringBuilder _outputStringBuilder;

        public TextWriterSpy()
        {
            _outputStringBuilder = new StringBuilder();
        }

        public override Encoding Encoding => Encoding.ASCII;

        public override void WriteLine(string s)
        {
            var sw = new StringWriter();
            sw.WriteLine(s);
            _outputStringBuilder.Append(sw);
        }

        public override void Write(string s)
        {
            var sw = new StringWriter();
            sw.Write(s);
            _outputStringBuilder.Append(sw);
        }

        public string Output => _outputStringBuilder.ToString();
    }
}
