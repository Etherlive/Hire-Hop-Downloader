using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Controls;

namespace Downloader_UI
{
    public class ControlWriter : TextWriter
    {
        private RichTextBox textbox;
        public ControlWriter(RichTextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Dispatcher.Invoke(() =>
            {
                textbox.AppendText(value.ToString());
                textbox.ScrollToEnd();
            });
        }

        public override void Write(string value)
        {
            textbox.Dispatcher.Invoke(() =>
            {
                textbox.AppendText(value);
                textbox.ScrollToEnd();
            });
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
