using System;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Drawing;
using Chat = System.Net;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace GearCamChat
{
    public partial class Client
    {
        private void HandleResponse(GAISReader gr)
        {
            RESPONSE id = (RESPONSE)gr.ReadInt32();
            if (id == RESPONSE.NICKNAME_TAKEN)
            {
                tcpClient.Close();
                MessageBox.Show("Nickname Taken!");
                pForm.SetVisibility(true);
            }
        }

        private void HandleVideoStart(GAISReader gr)
        {
            String nick = gr.ReadString();
            Program.General.Active = true;
        }

        private void HandleVideoEnd(GAISReader gr)
        {
            String nick = gr.ReadString();
            Program.General.Active = false;
        }

        private void HandleVideoUpdate(GAISReader gr)
        {
            String nick = gr.ReadString();
            Int32 size = gr.ReadInt32();
            Byte[] arr = gr.ReadBytes(size);

            if (arr.Length > 0)
            {
                Program.General.SetImage(arr);
            }
        }
    }
}
