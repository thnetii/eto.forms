using Microsoft.Extensions.Hosting;

namespace THNETII.EtoForms.HostedSample
{
    public class MainForm : Eto.Forms.Form
    {
        private MainForm() : base()
        {
            Size = new Eto.Drawing.Size(width: 640, height: 480);
        }

        public MainForm(IHostEnvironment environment) : this()
        {
            if (environment?.ApplicationName is string appName)
                Title = appName;
        }
    }
}
