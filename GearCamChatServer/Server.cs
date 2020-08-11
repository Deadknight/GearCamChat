using System.IO;
using System.Net;
using System;
using System.Threading;
using Chat = System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GearCamChatServer
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

    public struct ConnectionInfo
    {
        public String nickName;
        public TcpClient cl;
    }

    partial class Server : IDisposable
    {
        TcpListener lServer;
        public static List<ConnectionInfo> connections;
        Thread t;
        List<DoCommunicate> commList;

        public Server(String ip, Int32 port)
        {
            //create our nickname and nickname by connection variables
            connections = new List<ConnectionInfo>();
            commList = new List<DoCommunicate>();
            //create our TCPListener object
            IPAddress ipA = IPAddress.Parse(ip);
            lServer = new TcpListener(ipA, port);
            //check to see if the server is running
            //while (true) do the commands

            t = new Thread(new ThreadStart(Run));
            t.Start();
        }

        public void Run()
        {
            //start the chat server
            lServer.Start();
            while (true)
            {
                //check if there are any pending connection requests
                if (lServer.Pending())
                {
                    //if there are pending requests create a new connection
                    TcpClient Connection = lServer.AcceptTcpClient();
                    //create a new DoCommunicate Object
                    DoCommunicate comm = new DoCommunicate(Connection);
                    commList.Add(comm);
                }

                Thread.Sleep(100);
            }
        }

        public static void SendMsgToAll(String nickName, GAISReader gr)
        {
            List<ConnectionInfo> ToRemove = new List<ConnectionInfo>();

            GAISWriter gw = HandleReader(gr);

            foreach (ConnectionInfo c in connections)
            {
                if (c.nickName == nickName)
                {
                    if (gw == null)
                        ToRemove.Add(c);
                    else
                        continue;
                }
                if (gw != null)
                {
                    try
                    {
                        GAISWriter sendGW = new GAISWriter(c.cl.GetStream());
                        sendGW.Write(gw.ToArray());
                        sendGW.Flush();
                    }
                    catch
                    {
                        ToRemove.Add(c);
                    }
                }
            }

            foreach (ConnectionInfo c in ToRemove)
            {
                connections.Remove(c);
            }

            ToRemove.Clear();
        }

        private static GAISWriter HandleReader(GAISReader gr)
        {
            OPCODE op = (OPCODE)gr.ReadInt32();

            switch (op)
            {
                case OPCODE.REGISTER:
                    HandleRegister(gr);
                    return null;
                case OPCODE.REMOVE:
                    HandleRemove(gr);
                    return null;
                case OPCODE.VIDEO_START:
                    return HandleVideoStart(gr);
                case OPCODE.VIDEO_END:
                    return HandleVideoEnd(gr);
                case OPCODE.VIDEO_UPDATE:
                    return HandleVideoUpdate(gr);
                default:
                    return null;
            }            
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (DoCommunicate comm in commList)
            {
                comm.Dispose();
            }
            t.Abort();
        }

        #endregion
    }
}
