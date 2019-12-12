using System;

namespace THNETII.EtoForms.CrossPlatformSample
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var platform = Eto.Platform.Detect;
            using var app = new Eto.Forms.Application(platform);
            using var form = new MainForm();

            app.Run(form);
        }
    }
}
