
using Eto.Forms;

namespace THNETII.EtoForms.Controls
{
    public static class MessageBoxCustomDefaultButton
    {
        /// <inheritdoc cref="MessageBoxDefaultButton.Default"/>
        public const MessageBoxDefaultButton Default =
            MessageBoxDefaultButton.Default;

        /// <summary>
        /// Select the first, left-most button on the message box.
        /// Usually, this is the positive form button such as the button for
        /// <see cref="DialogResult.Ok"/> or <see cref="DialogResult.Yes"/>.
        /// </summary>
        public const MessageBoxDefaultButton Button1 =
            (MessageBoxDefaultButton)1;

        /// <summary>
        /// Select the second button on the message box.
        /// Usually, this is the first negative form button such as the button for
        /// <see cref="DialogResult.Cancel"/> or <see cref="DialogResult.No"/>.
        /// </summary>
        public const MessageBoxDefaultButton Button2 =
            (MessageBoxDefaultButton)2;

        /// <summary>
        /// Select the third button on the message box.
        /// </summary>
        public const MessageBoxDefaultButton Button3 =
            (MessageBoxDefaultButton)3;
    }
}
