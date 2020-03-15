using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Eto.Forms;

namespace THNETII.EtoForms.Controls
{
    public class MessageBoxCustom : Dialog<DialogResult>
    {
        private static readonly Dictionary<DialogResult, EventHandler<EventArgs>> ButtonClickHandlers =
            Enum.GetValues(typeof(DialogResult)).Cast<DialogResult>()
            .ToDictionary(r => r, r => new EventHandler<EventArgs>((sender, e) =>
            {
                var btn = (Button)sender;
                var dlg = btn.ParentWindow as Dialog<DialogResult>;
                dlg?.Close(r);
            }));

        private readonly MessageBoxButtons buttons;
        private readonly MessageBoxType type;
        private readonly MessageBoxDefaultButton defaultButton;

        [SuppressMessage("Globalization", "CA1303: Do not pass literals as localized parameters")]
        public MessageBoxCustom(MessageBoxButtons buttons,
            MessageBoxType type = MessageBoxType.Information,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Default)
        {
            _ = buttons switch
            {
                MessageBoxCustomButtons.OK => (true, defaultButton switch
                {
                    MessageBoxCustomDefaultButton.Default => true,
                    MessageBoxCustomDefaultButton.Button1 => true,
                    _ => throw new ArgumentException(
                        paramName: nameof(defaultButton),
                        message: $"Invalid value {defaultButton} of type {nameof(MessageBoxDefaultButton)} while parameter {nameof(buttons)} is {nameof(MessageBoxCustomButtons.OK)}"
                        ),
                }),
                MessageBoxCustomButtons.OKCancel => (true, defaultButton switch
                {
                    MessageBoxCustomDefaultButton.Default => true,
                    MessageBoxCustomDefaultButton.Button1 => true,
                    MessageBoxCustomDefaultButton.Button2 => true,
                    _ => throw new ArgumentException(
                        paramName: nameof(defaultButton),
                        message: $"Invalid value {defaultButton} of type {nameof(MessageBoxDefaultButton)} while parameter {nameof(buttons)} is {nameof(MessageBoxCustomButtons.OKCancel)}"
                        ),
                }),
                MessageBoxCustomButtons.YesNo => (true, defaultButton switch
                {
                    MessageBoxCustomDefaultButton.Default => true,
                    MessageBoxCustomDefaultButton.Button1 => true,
                    MessageBoxCustomDefaultButton.Button2 => true,
                    _ => throw new ArgumentException(
                        paramName: nameof(defaultButton),
                        message: $"Invalid value {defaultButton} of type {nameof(MessageBoxDefaultButton)} while parameter {nameof(buttons)} is {nameof(MessageBoxCustomButtons.YesNo)}"
                        ),
                }),
                MessageBoxCustomButtons.YesNoCancel => (true, defaultButton switch
                {
                    MessageBoxCustomDefaultButton.Default => true,
                    MessageBoxCustomDefaultButton.Button1 => true,
                    MessageBoxCustomDefaultButton.Button2 => true,
                    MessageBoxCustomDefaultButton.Button3 => true,
                    _ => throw new ArgumentException(
                        paramName: nameof(defaultButton),
                        message: $"Invalid value {defaultButton} of type {nameof(MessageBoxDefaultButton)} while parameter {nameof(buttons)} is {nameof(MessageBoxCustomButtons.YesNoCancel)}"
                        ),
                }),
                MessageBoxCustomButtons.RetryCancel => (true, defaultButton switch
                {
                    MessageBoxCustomDefaultButton.Default => true,
                    MessageBoxCustomDefaultButton.Button1 => true,
                    MessageBoxCustomDefaultButton.Button2 => true,
                    _ => throw new ArgumentException(
                        paramName: nameof(defaultButton),
                        message: $"Invalid value {defaultButton} of type {nameof(MessageBoxDefaultButton)} while parameter {nameof(buttons)} is {nameof(MessageBoxCustomButtons.RetryCancel)}"
                        ),
                }),
                MessageBoxCustomButtons.AbortRetryIgnore => (true, defaultButton switch
                {
                    MessageBoxCustomDefaultButton.Default => true,
                    MessageBoxCustomDefaultButton.Button1 => true,
                    MessageBoxCustomDefaultButton.Button2 => true,
                    MessageBoxCustomDefaultButton.Button3 => true,
                    _ => throw new ArgumentException(
                        paramName: nameof(defaultButton),
                        message: $"Invalid value {defaultButton} of type {nameof(MessageBoxDefaultButton)} while parameter {nameof(buttons)} is {nameof(MessageBoxCustomButtons.AbortRetryIgnore)}"
                        ),
                }),
                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(buttons),
                    actualValue: buttons,
                    message: $"Invalid value for parameter. Value is neither a value in the {nameof(MessageBoxButtons)} enumeration, nor one of the constants defined in the {nameof(MessageBoxCustomButtons)}."
                    ),
            };

            this.buttons = buttons;
            this.defaultButton = defaultButton;
            this.type = type;
        }

        protected override void OnLoad(EventArgs e)
        {
            var dftBtns = defaultButton switch
            {
                MessageBoxCustomDefaultButton.Default => buttons switch
                {
                    MessageBoxCustomButtons.OK => MessageBoxCustomDefaultButton.Button1,
                    MessageBoxCustomButtons.OKCancel => MessageBoxCustomDefaultButton.Button2,
                    MessageBoxCustomButtons.YesNo => MessageBoxCustomDefaultButton.Button2,
                    MessageBoxCustomButtons.YesNoCancel => MessageBoxCustomDefaultButton.Button3,
                    MessageBoxCustomButtons.RetryCancel => MessageBoxCustomDefaultButton.Button1,
                    MessageBoxCustomButtons.AbortRetryIgnore => MessageBoxCustomDefaultButton.Button1,
                    _ => throw new InvalidOperationException(),
                },
                var value => value
            };

            var btns = new Button[3];
            for (
                (int idx, DialogResult result) = (0, GetNextDialogResult(0, buttons));
                result != DialogResult.None;
                result = GetNextDialogResult(idx, buttons), idx++
                )
            {
                var btn = new Button(ButtonClickHandlers[result]);
                switch (result)
                {
                    case DialogResult.Ok:
                    case DialogResult.Yes:
                        PositiveButtons.Add(btn);
                        break;
                    case DialogResult.Cancel:
                    case DialogResult.No:
                    case DialogResult.Abort:
                    case DialogResult.Retry:
                    case DialogResult.Ignore:
                        NegativeButtons.Add(btn);
                        break;
                }
                switch (result)
                {
                    case DialogResult.No
                    when buttons == MessageBoxCustomButtons.YesNoCancel:
                    case DialogResult.Cancel:
                    case DialogResult.Abort:
                        AbortButton = btn;
                        break;
                }
                btns[idx] = btn;
            }

            switch (((int)dftBtns) - 1)
            {
                case int idx when idx >= 0 && idx < btns.Length:
                    DefaultButton = btns[idx];
                    break;
            }

            base.OnLoad(e);

            static DialogResult GetNextDialogResult(int idx, MessageBoxButtons buttons)
            {
                return buttons switch
                {
                    MessageBoxCustomButtons.OK => idx switch
                    {
                        0 => DialogResult.Ok,
                        _ => DialogResult.None,
                    },
                    MessageBoxCustomButtons.OKCancel => idx switch
                    {
                        0 => DialogResult.Ok,
                        1 => DialogResult.Cancel,
                        _ => DialogResult.None,
                    },
                    MessageBoxCustomButtons.YesNo => idx switch
                    {
                        0 => DialogResult.Yes,
                        1 => DialogResult.No,
                        _ => DialogResult.None,
                    },
                    MessageBoxCustomButtons.YesNoCancel => idx switch
                    {
                        0 => DialogResult.Yes,
                        1 => DialogResult.No,
                        2 => DialogResult.Cancel,
                        _ => DialogResult.None,
                    },
                    MessageBoxCustomButtons.RetryCancel => idx switch
                    {
                        0 => DialogResult.Retry,
                        1 => DialogResult.Cancel,
                        _ => DialogResult.None,
                    },
                    MessageBoxCustomButtons.AbortRetryIgnore => idx switch
                    {
                        0 => DialogResult.Abort,
                        1 => DialogResult.Retry,
                        2 => DialogResult.Ignore,
                        _ => DialogResult.None,
                    },
                    _ => DialogResult.None
                };
            }
        }
    }
}
