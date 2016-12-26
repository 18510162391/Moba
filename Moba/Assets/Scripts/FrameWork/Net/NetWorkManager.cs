using System;
using System.Net.Sockets;
using UnityEngine;

public class NetWorkManager
{
    private Socket client;
    private IMsgHander m_pMsgHander;
    private NetSetting m_pNetSetting;

    public NetWorkManager(IMsgHander msgHander, NetSetting netSetting)
    {
        this.m_pMsgHander = msgHander;
        this.m_pNetSetting = netSetting;

        if (this.m_pMsgHander == null || this.m_pNetSetting == null)
        {
            Debug.LogError("NetWorkManager Init Error");
            return;
        }
    }

    public void Connect()
    {
        if (Connected())
        {
            Debug.LogError("Socket has connected, can not connect");
        }

        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //client.ConnectAsync();

    }

    private bool Connected()
    {
        return false;
    }
}
