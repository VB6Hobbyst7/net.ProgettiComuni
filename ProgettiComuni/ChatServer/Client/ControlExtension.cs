﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public static class RichTextBoxExtensions
    {
    
        public static void AppendText(this RichTextBox box, string text, Color color, bool Right = false)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.SelectionAlignment = HorizontalAlignment.Left;
            if (Right)
                box.SelectionAlignment = HorizontalAlignment.Right;

            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
