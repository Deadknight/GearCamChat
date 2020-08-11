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

namespace GearCamChatServer
{
    public partial class Server
    {
        private static void HandleRegister(GAISReader gr)
        {
            /*RESPONSE id = (RESPONSE)gr.ReadInt32();
            if (id == RESPONSE.NICKNAME_TAKEN)
            {
                tcpClient.Close();
                MessageBox.Show("Nickname Taken!");
                pForm.SetVisibility(true);
            }*/
        }

        private static void HandleRemove(GAISReader gr)
        {
            /*RESPONSE id = (RESPONSE)gr.ReadInt32();
            if (id == RESPONSE.NICKNAME_TAKEN)
            {
                tcpClient.Close();
                MessageBox.Show("Nickname Taken!");
                pForm.SetVisibility(true);
            }*/
        }

        private static GAISWriter HandleVideoStart(GAISReader gr)
        {
            GAISWriter gw = new GAISWriter();
            gw.Write((Int32)OPCODE.VIDEO_START);
            gw.Write(gr.ReadString());
            return gw;
        }

        private static GAISWriter HandleVideoEnd(GAISReader gr)
        {
            GAISWriter gw = new GAISWriter();
            gw.Write((Int32)OPCODE.VIDEO_END);
            gw.Write(gr.ReadString());
            return gw;
        }

        private static GAISWriter HandleVideoUpdate(GAISReader gr)
        {
            GAISWriter gw = new GAISWriter();
            gw.Write((Int32)OPCODE.VIDEO_UPDATE);
            gw.Write(gr.ReadString());
            Int32 size = gr.ReadInt32();
            Byte[] arr = gr.ReadBytes(size);

            if (arr.Length > 0)
            {
                gw.Write(size);
                gw.Write(arr);
                return gw;
            }

            return null;
        }
    }
}
