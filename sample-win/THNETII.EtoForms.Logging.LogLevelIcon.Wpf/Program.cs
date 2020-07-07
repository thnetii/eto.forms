using System;

namespace THNETII.EtoForms.Logging.LogLevelIcon.Wpf
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var etoPlatform = Eto.Platform.Detect;
            var etoApplication = new Eto.Forms.Application(etoPlatform);
            etoApplication.Run(new MainForm());
        }
    }
}
