using System;
using System.Diagnostics.CodeAnalysis;

namespace THNETII.EtoForms.Hosting
{
    public class EtoFormsOptions
    {
        /// <summary>
        /// Indicates if host lifetime status messages should be supressed such
        /// as on startup. The default is <see langword="false"/>.
        /// </summary>
        public bool SuppressStatusMessages { get; set; }

        public Type MainForm { get; set; }

        [SuppressMessage("Usage", "CA2208: Instantiate argument exceptions correctly")]
        public void Validate()
        {
            switch (MainForm)
            {
                case null:
                case Type tForm when typeof(Eto.Forms.Form).IsAssignableFrom(tForm):
                case Type tDialog when typeof(Eto.Forms.Dialog).IsAssignableFrom(tDialog):
                    break;
                default:
                    throw new ArgumentException($"The type specified for the {nameof(MainForm)} property, must either be null, or a type that is assingable to {typeof(Eto.Forms.Form)} or {typeof(Eto.Forms.Dialog)}.", nameof(MainForm));
            }
        }
    }
}
