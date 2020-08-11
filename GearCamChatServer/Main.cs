using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GearCamChatServer
{
    public partial class Main : Form
    {
        Server s;

        public Main()
        {
            InitializeComponent();
            s = null;
        }

        private void butHost_Click(object sender, EventArgs e)
        {
            txtIp.Enabled = false;
            txtPort.Enabled = false;
            butHost.Enabled = false;
            butShutdown.Enabled = true;
            if(s == null)
                s = new Server(txtIp.Text, Convert.ToInt32(txtPort.Text));
        }

        private void butShutdown_Click(object sender, EventArgs e)
        {
            txtIp.Enabled = true;
            txtPort.Enabled = true;
            butHost.Enabled = true;
            butShutdown.Enabled = false;
            if (s != null)
                s.Dispose();
        }
    }
}