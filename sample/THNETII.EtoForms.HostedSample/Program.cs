using Microsoft.Extensions.Hosting;

using System;

namespace THNETII.EtoForms.HostedSample
{
    public static class Program
    {
        [STAThread]
#if NETCOREAPP
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .RunEtoForm<MainForm>();
        }
#else
        public static void Main()
        {
            new HostBuilder().RunEtoForm<MainForm>();
        }
#endif
    }
}
