using Microsoft.Extensions.Logging;

namespace THNETII.EtoForms.Logging.LogLevelIcon.Wpf
{
    public class MainForm : Eto.Forms.Form
    {
        public MainForm() : base()
        {
            var logLevelStepper = new Eto.Forms.NumericStepper
            {
                DecimalPlaces = 0,
                MinValue = 0,
                Increment = 1,
                Value = 0
            };
            var logLevelBinding = logLevelStepper.ValueBinding.Convert(
                d => (LogLevel)d,
                l => (double)l
                );
            var logLevelTextBinding = logLevelBinding.Convert(l => l.ToString());
            var logLevelLabel = new Eto.Forms.Label();
            logLevelLabel.TextBinding.Bind(logLevelTextBinding, Eto.Forms.DualBindingMode.OneWay);

            var contentStackLayout = new Eto.Forms.StackLayout(
                new Eto.Forms.StackLayoutItem(logLevelStepper),
                new Eto.Forms.StackLayoutItem(logLevelLabel)
                )
            {
                HorizontalContentAlignment = Eto.Forms.HorizontalAlignment.Center,
                Orientation = Eto.Forms.Orientation.Vertical
            };

            Padding = new Eto.Drawing.Padding(40);
            Size = new Eto.Drawing.Size(-1, -1);
            Content = contentStackLayout;
            var titleBinding = new Eto.Forms.BindableBinding<MainForm, string>(this,
                control => control.Title,
                (control, title) => control.Title = title);
            titleBinding.Bind(logLevelTextBinding, Eto.Forms.DualBindingMode.OneWay);
        }
    }
}
