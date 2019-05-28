using System;

namespace THNETII.EtoForms.RemoteAuthUI
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var etoPlatform = Eto.Platform.Detect;
            var etoApplication = new Eto.Forms.Application(etoPlatform);

            using (var mainForm = new MainForm())
                etoApplication.Run(new MainForm());
        }
    }
}
