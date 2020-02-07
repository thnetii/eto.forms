using System;
using System.CommandLine.Builder;

namespace THNETII.EtoForms.CmdParserHostedSample
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var cmdParser = new CommandLineBuilder()
                .UseDefaults();
        }
    }
}
