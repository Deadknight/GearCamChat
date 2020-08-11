using System.IO;
using System.Net;
using System;
using System.Threading;
using Chat = System.Net;
using System.Collections;
using System.Net.Sockets;

namespace GearCamChatServer
{
    class DoCommunicate : IDisposable
    {
        TcpClient client;
        String nickName;
        GAISReader gr;
        bool connected;
        Thread chatThread;

        public DoCommunicate(TcpClient tcpClient)
        {
            //create our TcpClient
            client = tcpClient;
            //create a new thread
            Thread sendThread = new Thread(new ThreadStart(startFirst));
            //start the new thread
            sendThread.Start();
            connected = false;
        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    if (client.Available > 0)
                    {
                        //send our message
                        Server.SendMsgToAll(nickName, gr);
                    }

                    Thread.Sleep(100);
                }
            }
            catch
            {
                client.Close();
                Thread.CurrentThread.Abort();
            }
        }

        private void startFirst()
        {
            while (!connected)
            {
                try
                {
                    if (client.Available > 0)
                    {
                        gr = new GAISReader(client.GetStream());

                        Int32 id = gr.ReadInt32();
                        nickName = gr.ReadString();

                        foreach (ConnectionInfo c in Server.connections)
                        {
                            if (c.nickName == nickName)
                            {
                                GAISWriter gw = new GAISWriter();
                                gw.Write((Int32)OPCODE.RESPONSE);
                                gw.Write((Int32)RESPONSE.NICKNAME_TAKEN);
                                GAISWriter gwSend = new GAISWriter(client.GetStream());
                                gwSend.Write(gw.ToArray());
                                gwSend.Flush();
                                return;
                            }
                        }
                        ConnectionInfo ci = new ConnectionInfo();
                        ci.nickName = nickName;
                        ci.cl = client;
                        Server.connections.Add(ci);

                        //create a new thread for this user
                        chatThread = new Thread(new ThreadStart(Run));
                        //start the thread
                        chatThread.Start();

                        connected = true;
                    }
                }
                catch
                {
                    client.Close();
                    connected = true;
                }

                Thread.Sleep(100);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            client.Close();
            chatThread.Abort();
        }

        #endregion
    }
}

