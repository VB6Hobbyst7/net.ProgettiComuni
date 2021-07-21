using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Security
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = Security.StringCipher.Encrypt(textBox2.Text, textBox1.Text);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            textBox5.Text = Security.StringCipher.Decrypt(textBox4.Text, textBox1.Text);
        }
    }
}
