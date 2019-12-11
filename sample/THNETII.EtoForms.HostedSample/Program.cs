using Microsoft.Extensions.Hosting;

using System.Threading.Tasks;

namespace THNETII.EtoForms.HostedSample
{
    public static class Program
    {
#if NETCOREAPP
        public static Task Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .RunEtoFormAsync<MainForm>();
        }
#else
        public static Task Main()
        {
            return new HostBuilder()
                .RunEtoFormAsync<MainForm>();
        }
#endif
    }
}
