namespace THNETII.EtoForms.CrossPlatformSample
{
    public class MainForm : Eto.Forms.Form
    {
        public MainForm() : base()
        {
            Size = new Eto.Drawing.Size(width: 640, height: 480);
            Title = typeof(MainForm).Assembly.GetName().Name;
        }
    }
}
