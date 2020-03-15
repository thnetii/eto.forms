using System;
using System.Collections.Generic;
using System.Text;
using Eto.Drawing;
using Eto.Forms;
using THNETII.EtoForms.Controls;

namespace THNETII.EtoForms.ControlsSample
{
    public class CustomMessageBoxForm : Dialog
    {
        public CustomMessageBoxForm() : base()
        {
            var buttonsDropDown = new DropDown
            {
                Items =
                {
                    new ListItem
                    {
                        Text = nameof(MessageBoxCustomButtons.OK),
                        Tag = MessageBoxCustomButtons.OK
                    },
                    new ListItem
                    {

                    }
                }
            };
            buttonsDropDown.SelectedValueBinding.Bind(
                () => MessageBoxButtons,
                bt => MessageBoxButtons = (MessageBoxButtons)bt
                );
            var defaultDropDown = new DropDown
            {
                Items =
                {
                    new ListItem
                    {
                        Text = nameof(MessageBoxCustomDefaultButton.Default),
                        Tag = MessageBoxCustomDefaultButton.Default
                    }
                }
            };
            defaultDropDown.SelectedValueBinding.Bind(
                () => MessageBoxDefaultButton,
                bt => MessageBoxDefaultButton = (MessageBoxDefaultButton)bt
                );
            var tableLayout = new TableLayout
            {
                Rows =
                {
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell
                            {
                                Control = new Label
                                {
                                    Text = nameof(MessageBoxButtons)
                                }
                            },
                            new TableCell(buttonsDropDown)
                        }
                    },
                    new TableRow
                    {
                        Cells =
                        {
                            new TableCell
                            {
                                Control = new Label
                                {
                                    Text = nameof(MessageBoxDefaultButton)
                                }
                            },
                            new TableCell(defaultDropDown)
                        }
                    }
                }
            };
            Content = tableLayout;

            DefaultButton = new Button(OnShowMessageBoxButtonClick);
        }

        public MessageBoxButtons MessageBoxButtons { get; set; } =
            MessageBoxCustomButtons.OK;

        public MessageBoxDefaultButton MessageBoxDefaultButton { get; set; } =
            MessageBoxCustomDefaultButton.Default;

        private void OnShowMessageBoxButtonClick(object? sender, EventArgs e)
        {
            DialogResult res;
            try
            {
                using var dlg = new MessageBoxCustom(
                        MessageBoxButtons,
                        MessageBoxType.Information,
                        MessageBoxDefaultButton);
                res = dlg.ShowModal(this);
            }
            catch (Exception excpt)
            {
                res = MessageBox.Show(this, excpt.ToString(),
                    excpt.GetType().ToString(), MessageBoxType.Error);
            }
        }
    }
}
