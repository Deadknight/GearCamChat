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
    public enum OPCODE
    {
        REGISTER = 1,
        RESPONSE = 2,
        REMOVE = 3,
        VIDEO_START = 4,
        VIDEO_UPDATE = 5,
        VIDEO_END = 6
    }

    public enum RESPONSE
    {
        OK,
        NICKNAME_TAKEN
    }

    public partial class Client : IDisposable
    {
        private bool connected;

        Thread t;
        TcpClient tcpClient;
        String Nick;
        MainForm pForm;

        //Constructor
        //It starts the client thread and connects to server
        public Client(String nick, String ip, Int32 port, MainForm form)
        {
            try
            {
                pForm = form;
                tcpClient = new TcpClient();
                Nick = nick;
                tcpClient.Connect(ip, port);
                connected = true;
                t = new Thread(new ThreadStart(Run));
                t.Start();
                GAISWriter gw = new GAISWriter();
                gw.Write((Int32)OPCODE.REGISTER);
                gw.Write(nick);
                SendData(gw);
            }
            catch (Exception e)
            {
                MessageBox.Show("Cannot connect" + e.Message);
                pForm.SetVisibility(true);
            }
        }

        public void SendData(GAISWriter gw)
        {
            if(connected)
            {
                GAISWriter gwSend = new GAISWriter(tcpClient.GetStream());
                gwSend.Write(gw.ToArray());
                gwSend.Flush();
            }
        }

        public void SendData(Byte[] arr)
        {
            if (connected)
            {
                GAISWriter gw = new GAISWriter();
                gw.Write((Int32)OPCODE.VIDEO_UPDATE);
                gw.Write(Nick);
                gw.Write(arr.Length);
                gw.Write(arr);
                GAISWriter gwSend = new GAISWriter(tcpClient.GetStream());
                gwSend.Write(gw.ToArray());
                gwSend.Flush();
            }
        }

        private void Run()
        {
            while (true)
            {
                try
                {
                    if (tcpClient.Available > 0)
                    {
                        GAISReader gr = new GAISReader(tcpClient.GetStream());

                        OPCODE op = (OPCODE)gr.ReadInt32();

                        switch (op)
                        {
                            case OPCODE.RESPONSE:
                                HandleResponse(gr);
                                break;
                            case OPCODE.VIDEO_START:
                                HandleVideoStart(gr);
                                break;
                            case OPCODE.VIDEO_END:
                                HandleVideoEnd(gr);
                                break;
                            case OPCODE.VIDEO_UPDATE:
                                HandleVideoUpdate(gr);
                                break;
                        }
                    }
                }
                catch
                {
                    tcpClient.Close();
                }
                
                Thread.Sleep(100);
            }
        }

        public bool IsConnected() { return connected; }

        #region IDisposable Members

        public void Dispose()
        {
            tcpClient.Close();
            t.Abort();
        }

        #endregion
    }
}