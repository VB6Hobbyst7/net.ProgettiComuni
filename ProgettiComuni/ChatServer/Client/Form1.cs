using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using Client.ChatService;

namespace Client
{
    public partial class frmClient : Form
    {
        ReceiveClient rc = null;
        string myName;
        //public string User = "";
        int tentativi = 0;
       
        public frmClient()
        {
            InitializeComponent();
            this.FormClosing+=new FormClosingEventHandler(frmClient_FormClosing);
            this.txtSend.KeyPress += new KeyPressEventHandler(txtSend_KeyPress);
            
        }

        void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            int keyValue = (int)e.KeyChar;

            if (keyValue == 13)
                SendMessage();
            
        }

        private void frmClient_FormClosing(object sender, EventArgs e)
        {
            rc.Stop(myName);
        }

        private void frmClient_Load(object sender, EventArgs e)
        {
            txtMsgs.Enabled = false;
            txtSend.Enabled = false;
            btnSend.Enabled = false;

            if (Global.User != "")
            {
                txtUserName.Text = Global.User;
                btnLogin_Click(null, null);
            }
        }

        void rc_ReceiveMsg(string sender, string msg)
        {
            string strMSG = "\r\n{0} {1} {2}";

            if (msg.Length > 0)
            {
                strMSG = string.Format(strMSG, sender, " >>>> ", msg);
                if (sender == myName)
                {
                    strMSG = string.Format(strMSG, sender, " [io] > ", msg);
                    richTextBox1.AppendText(strMSG, Color.Black);
                }
                else
                {
                    richTextBox1.AppendText(strMSG, Color.Red, true);
                }

                txtMsgs.Text += strMSG;
                
            }
                // sender +" > "+ msg;
        }

        void rc_NewNames(object sender, List<string> names)
        {
            lstClients.Items.Clear();
            foreach (string name in names)
            {
                if (name!=myName)
                    lstClients.Items.Add(name);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            tentativi++;

            if (tentativi > 3)
            {
                MessageBox.Show("Il servizio potrebbe essere spento o non raggiungibile!");
                tentativi = 0;
                return;
            }

            try
            {
                string strMSG = "\r\n{0} {1} {2}";
                strMSG = string.Format(strMSG, myName, " [io] > ", txtSend.Text);

                if (lstClients.Items.Count != 0)
                {
                    txtMsgs.Text += Environment.NewLine + myName + " [io] > " + txtSend.Text;
                    if (lstClients.SelectedItems.Count == 0)
                    {
                        rc.SendMessage(txtSend.Text, myName, lstClients.Items[0].ToString());
                        richTextBox1.AppendText(strMSG, Color.Black, false);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(lstClients.SelectedItem.ToString()))
                        {
                            rc.SendMessage(txtSend.Text, myName, lstClients.SelectedItem.ToString());
                            richTextBox1.AppendText(strMSG, Color.Black);
                        }
                    }
                    txtSend.Clear();
                }
            }
            catch (Exception ex)
            {
                btnLogin_Click(null, null);
                SendMessage();
                
            }

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text.Length > 0)
            {
                txtMsgs.Enabled = true;
                txtSend.Enabled = true;
                btnSend.Enabled = true;

                myName = txtUserName.Text.Trim();

                rc = new ReceiveClient();
                rc.Start(rc, myName);

                rc.NewNames += new GotNames(rc_NewNames);
                rc.ReceiveMsg += new ReceviedMessage(rc_ReceiveMsg);
            }
            else
            {
                txtMsgs.Enabled = false;
                txtSend.Enabled = false;
                btnSend.Enabled = false;
            }
        }

        

    }
   
}
