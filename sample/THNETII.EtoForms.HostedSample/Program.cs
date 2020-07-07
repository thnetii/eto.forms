using Microsoft.Extensions.Hosting;

using System;

namespace THNETII.EtoForms.HostedSample
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .RunEtoForm<MainForm>();
        }
    }
}
