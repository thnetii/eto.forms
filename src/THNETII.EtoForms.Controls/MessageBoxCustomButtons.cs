
using Eto.Forms;

namespace THNETII.EtoForms.Controls
{
    /// <summary>
    /// Custom Message box buttons for methods of <see cref="MessageBoxCustom"/>.
    /// </summary>
    public static class MessageBoxCustomButtons
    {
        /// <inheritdoc cref="MessageBoxButtons.OK"/>
        public const MessageBoxButtons OK = MessageBoxButtons.OK;

        /// <inheritdoc cref="MessageBoxButtons.OKCancel"/>
        public const MessageBoxButtons OKCancel = MessageBoxButtons.OKCancel;

        /// <inheritdoc cref="MessageBoxButtons.YesNo"/>
        public const MessageBoxButtons YesNo = MessageBoxButtons.YesNo;

        /// <inheritdoc cref="MessageBoxButtons.YesNoCancel"/>
        public const MessageBoxButtons YesNoCancel = MessageBoxButtons.YesNoCancel;

        /// <summary>Abort, Retry and Ignore buttons</summary>
        public const MessageBoxButtons AbortRetryIgnore =
            (MessageBoxButtons)(-(((int)MessageBoxButtons.YesNoCancel) + 1));

        /// <summary>Retry and Cancel buttons</summary>
        public const MessageBoxButtons RetryCancel =
            (MessageBoxButtons)(-(((int)MessageBoxButtons.YesNoCancel) + 2));
    }
}
